# Changelog

Format inspired by [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Added

- Art pieces, graves and meditation foci now count as recreation spots for manual invitations and
  autonomous events (setting "Extend building invitations").
- "Invite ... to an activity" on a colonist, for recreation with no building to click: walks,
  skygazing, swimming, reading, meditation, prayer, drugs and treats (setting "Enable the
  placeless activity menu", with a window to choose the offered activities).
- Board games can be played sitting on the ground when no chair is free, up to the number of
  players the game allows.
- A failed manual invitation now says why (one artwork per viewer, no seat left, no free spot to
  meditate) instead of always "Not enough space".
- French translation.

### Fixed

- Research and tailoring benches are no longer taken for seats (the old check accepted any
  `defName` containing "bench" or "seat").
- A hauler, builder or cleaner holding a reservation on a table no longer makes it look full and
  blocks the shared break: only reservations of the recreation itself are counted.
- A warning is logged when the private vanilla methods the mod relies on cannot be found.

### Changed

- The settings window scrolls.
- Sources moved to `Source/`, SDK-style project (net48, `Krafs.Rimworld.Ref`), MIT `LICENSE` added.
