"""Analyze CsLint JSON output for false positive patterns.

Usage:
  python analyze.py <results.json>              # summary + suspicious patterns
  python analyze.py <results.json> --details    # also print file:line for every violation
  python analyze.py <results.json> --details CSLINT210 CSLINT104  # details for specific rules only
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
    args = parser.parse_args()

    data = load_results(args.results)

    if not data:
        print("No violations found. CsLint reported a clean run.")
        sys.exit(0)

    print_summary(data)
    suspicious = find_suspicious(data)
    print_suspicious(suspicious)

    if args.details is not None:
        rule_filter = args.details if args.details else None
        print_details(data, rule_filter)


if __name__ == "__main__":
    main()
