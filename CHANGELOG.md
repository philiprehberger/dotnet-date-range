# Changelog

## 0.2.1 (2026-03-31)

- Standardize README to 3-badge format with emoji Support section
- Update CI actions to v5 for Node.js 24 compatibility

## 0.2.0 (2026-03-28)

- Add `MergeAll(IEnumerable<DateRange>)` static method to merge overlapping ranges into minimal non-overlapping set
- Add `BusinessDays()` method to count weekdays within a range excluding weekends
- Add `ExcludeWeekends()` method to return weekday-only sub-ranges
- Add duration comparison helpers: `IsShorterThan(TimeSpan)`, `IsLongerThan(TimeSpan)`, `DurationEquals(TimeSpan)`
- Add missing GitHub issue templates, dependabot configuration, and pull request template
- Fix README compliance: add missing badges, Support section, and correct section order

## 0.1.3 (2026-03-26)

- Add Sponsor badge and fix License link format in README

## 0.1.2 (2026-03-24)

- Add unit tests
- Add test step to CI workflow

## 0.1.1 (2026-03-23)

- Fix NuGet badge URL format

## 0.1.0 (2026-03-22)

- Initial release
- Immutable DateRange readonly record struct with Start, End, Duration
- Overlap detection, intersection, union, and gap computation
- Contains checks for points and sub-ranges
- Collection extensions for merging, gap finding, and overlap detection
