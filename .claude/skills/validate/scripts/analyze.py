"""Analyze CsLint JSON output for false positive patterns.

Usage:
  python analyze.py <results.json>                              # summary + suspicious patterns
  python analyze.py <results.json> --details                    # summary + file:line for every violation
  python analyze.py <results.json> --details CSLINT210          # summary + file:line for specific rules
  python analyze.py <results.json> --details-only CSLINT210     # file:line ONLY (no summary, pipe-friendly)
"""

import argparse
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path


def load_results(path: str) -> list[dict]:
    with open(path) as f:
        return json.load(f)


def print_summary(data: list[dict]) -> None:
    print(f"Total violations: {len(data)}")
    print()

    by_rule = Counter(d["ruleId"] for d in data)
    print("By rule ID (sorted by count):")
    for rule, count in by_rule.most_common():
        print(f"  {rule}: {count}")
    print()

    rule_msgs: dict[str, Counter] = defaultdict(Counter)
    for d in data:
        rule_msgs[d["ruleId"]][d["message"]] += 1

    for rule_id in sorted(rule_msgs.keys()):
        total = sum(rule_msgs[rule_id].values())
        print(f"{rule_id} ({total}) — top messages:")
        for msg, count in rule_msgs[rule_id].most_common(5):
            print(f"  [{count}] {msg}")
        remaining = len(rule_msgs[rule_id]) - 5
        if remaining > 0:
            print(f"  ... and {remaining} more distinct messages")
        print()


def print_details(data: list[dict], rule_filter: list[str] | None = None) -> None:
    """Print file:line for every violation, grouped by rule ID."""
    by_rule: dict[str, list[dict]] = defaultdict(list)
    for d in data:
        by_rule[d["ruleId"]].append(d)

    print("=" * 60)
    print("DETAILED VIOLATIONS")
    print("=" * 60)
    print()

    for rule_id in sorted(by_rule.keys()):
        if rule_filter and rule_id not in rule_filter:
            continue

        items = by_rule[rule_id]
        print(f"=== {rule_id} ({len(items)}) ===")
        for d in items:
            print(f"  {d['filePath']}:{d['line']} -- {d['message']}")
        print()


def find_suspicious(data: list[dict]) -> dict[str, list[dict]]:
    """Flag violations matching known false-positive patterns.

    Returns a dict of pattern_name -> list of matching violations.
    """
    suspicious: dict[str, list[dict]] = defaultdict(list)

    for d in data:
        rid = d["ruleId"]
        msg = d["message"]
        fp = d["filePath"]

        # Verbatim identifiers: message contains '@' inside the quoted name
        if rid in ("CSLINT100", "CSLINT101", "CSLINT102", "CSLINT103", "CSLINT104", "CSLINT105"):
            match = re.search(r"'(@\w+)'", msg)
            if match:
                suspicious["verbatim_identifier"].append(d)

        # Empty identifier name
        if "''" in msg:
            suspicious["empty_identifier"].append(d)

        # CSLINT103: PascalCase parameters (might be primary ctor / record params)
        if rid == "CSLINT103" and msg.startswith("parameter '"):
            name_match = re.search(r"parameter '(\w+)'", msg)
            if name_match and name_match.group(1)[0].isupper():
                suspicious["pascal_case_parameter"].append(d)

        # Generated/auto-generated files
        if any(p in fp for p in (".g.cs", ".Generated.", "AssemblyInfo.cs", ".designer.cs")):
            suspicious["generated_file"].append(d)

        # CA1805: "Do not initialize field" on const fields (constants require initializers)
        if rid == "CA1805" and "Do not initialize field" in msg:
            suspicious["possible_const_field_init"].append(d)

        # CSLINT251: "Field should be private" — may be in struct/interop context
        if rid == "CSLINT251":
            suspicious["possible_struct_field"].append(d)

        # CSLINT106: digit-suffixed type params (T0, T1, T2) are valid convention
        if rid == "CSLINT106":
            name_match = re.search(r"'(T\d+)'", msg)
            if name_match:
                suspicious["digit_suffixed_type_param"].append(d)

    # Build per-file and per-rule indexes for cross-cutting checks
    by_file: dict[str, list[dict]] = defaultdict(list)
    by_rule: dict[str, list[dict]] = defaultdict(list)
    for d in data:
        by_file[d["filePath"]].append(d)
        by_rule[d["ruleId"]].append(d)

    # Naming violations in interop files (files containing DllImport/LibraryImport/StructLayout)
    interop_files: set[str] = set()
    for fp, items in by_file.items():
        # Check if any violation in this file hints at interop context
        # (we can't read source, but file names and other rule hits are clues)
        has_interop_hint = False
        for d in items:
            # CSLINT251 in a file strongly hints at structs with public fields
            if d["ruleId"] == "CSLINT251":
                has_interop_hint = True
                break
            # Field naming violations mentioning ALL_CAPS or native-style names
            if d["ruleId"] == "CSLINT104" and d["message"]:
                name_match = re.search(r"'(\w+)'", d["message"])
                if name_match:
                    name = name_match.group(1)
                    # Names with underscores or ALL_CAPS suggest native/interop
                    if "_" in name or (name.isupper() and len(name) > 2):
                        has_interop_hint = True
                        break
        if has_interop_hint:
            interop_files.add(fp)

    # Flag naming rule violations (CSLINT100-106) co-located in interop files
    naming_rules = {"CSLINT100", "CSLINT101", "CSLINT102", "CSLINT103", "CSLINT104", "CSLINT105", "CSLINT106"}
    for fp in interop_files:
        for d in by_file[fp]:
            if d["ruleId"] in naming_rules and d not in suspicious.get("verbatim_identifier", []):
                suspicious["naming_in_interop_file"].append(d)

    # Suggestion rules with outlier counts (5x+ median suggests rule is too aggressive)
    suggestion_rules = {
        "CSLINT200", "CSLINT201", "CSLINT208", "CSLINT209", "CSLINT210",
        "CSLINT216", "CSLINT218", "CSLINT220", "CSLINT222", "IDE0004",
    }
    suggestion_counts = {rid: len(items) for rid, items in by_rule.items() if rid in suggestion_rules and len(items) > 0}
    if len(suggestion_counts) >= 3:
        median_count = sorted(suggestion_counts.values())[len(suggestion_counts) // 2]
        if median_count > 0:
            for rid, count in suggestion_counts.items():
                if count >= median_count * 5:
                    for d in by_rule[rid]:
                        suspicious["suggestion_rule_outlier"].append(d)

    # Concentration check: rules where >80% of violations come from <3 files
    by_rule: dict[str, list[dict]] = defaultdict(list)
    for d in data:
        by_rule[d["ruleId"]].append(d)

    for rid, items in by_rule.items():
        if len(items) < 5:
            continue
        file_counts = Counter(d["filePath"] for d in items)
        top_files = file_counts.most_common(3)
        top_count = sum(c for _, c in top_files)
        if top_count / len(items) > 0.8 and len(file_counts) <= 3:
            for d in items:
                suspicious["concentrated_violations"].append(d)

    return suspicious


def print_suspicious(suspicious: dict[str, list[dict]]) -> None:
    if not suspicious:
        print("No suspicious patterns detected.")
        return

    print("=" * 60)
    print("SUSPICIOUS PATTERNS (potential false positives)")
    print("=" * 60)
    print()

    labels = {
        "verbatim_identifier": "Verbatim identifiers (@prefix) — may indicate .Text vs .ValueText bug",
        "empty_identifier": "Empty identifier names — CsLint reading wrong token",
        "pascal_case_parameter": "PascalCase parameters — may be primary ctor / record params",
        "generated_file": "Violations in generated/auto-generated files",
        "possible_const_field_init": "Unnecessary init on possible const fields — constants require initializers",
        "possible_struct_field": "Field visibility in possible structs — struct fields are commonly public",
        "digit_suffixed_type_param": "Digit-suffixed type parameters (T0, T1) — valid naming convention",
        "naming_in_interop_file": "Naming violations in likely interop files — names must match native APIs",
        "suggestion_rule_outlier": "Suggestion rule with 5x+ median count — rule may be too aggressive",
        "concentrated_violations": "Highly concentrated violations (>80% in ≤3 files) — check for unhandled context",
    }

    for pattern, items in suspicious.items():
        label = labels.get(pattern, pattern)
        print(f"### {label} ({len(items)} violations)")
        print()

        # Group by rule
        by_rule: dict[str, list[dict]] = defaultdict(list)
        for d in items:
            by_rule[d["ruleId"]].append(d)

        for rid in sorted(by_rule.keys()):
            rule_items = by_rule[rid]
            print(f"  {rid}: {len(rule_items)} violations")
            for d in rule_items[:5]:
                short_path = Path(d["filePath"]).name
                print(f"    {short_path}:{d['line']} — {d['message']}")
            if len(rule_items) > 5:
                print(f"    ... and {len(rule_items) - 5} more")
        print()

    # Summary line for agent to use
    total_suspicious = sum(len(v) for v in suspicious.values())
    print(f"Total suspicious: {total_suspicious}")
    print()
    print("NEXT: Read source code for 3-5 examples per pattern to confirm or dismiss.")


def main() -> None:
    parser = argparse.ArgumentParser(description="Analyze CsLint JSON results for false positives.")
    parser.add_argument("results", help="Path to CsLint JSON results file")
    parser.add_argument(
        "--details",
        nargs="*",
        default=None,
        metavar="RULE_ID",
        help="Print file:line for every violation. Optionally filter by rule IDs (e.g. --details CSLINT210 CSLINT104)",
    )
    parser.add_argument(
        "--details-only",
        nargs="*",
        default=None,
        metavar="RULE_ID",
        help="Print ONLY file:line details (no summary). Pipe-friendly. Optionally filter by rule IDs.",
    )
    args = parser.parse_args()

    data = load_results(args.results)

    if not data:
        print("No violations found. CsLint reported a clean run.")
        sys.exit(0)

    # --details-only: skip summary, print only file:line details
    if args.details_only is not None:
        rule_filter = args.details_only if args.details_only else None
        print_details(data, rule_filter)
        return

    print_summary(data)
    suspicious = find_suspicious(data)
    print_suspicious(suspicious)

    if args.details is not None:
        rule_filter = args.details if args.details else None
        print_details(data, rule_filter)


if __name__ == "__main__":
    main()
