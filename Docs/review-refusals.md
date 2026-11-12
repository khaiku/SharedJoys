# Review refusals

ChatGPT suggestions refused, one line each: key or text, proposal, reason, date. Sent with every `/review-translation` page so the same proposal is not made again.

- `Blues_SweepOffFeet.logRulesInitiator.rulesStrings.0` (French): keep `[RECIPIENT_possessive]` ("loin de [RECIPIENT_possessive] pieds", "a fait perdre [RECIPIENT_possessive] moyens"): the token renders "son" or "sa" and gives "sa pieds"; "perdre ses moyens" changes the meaning; the French idiom needs no possessive. Refused 2026-11-12 (proposed four times).
- `Blues_RomanticJoy_Recipient.stages.swept_off_my_feet.description` (French): "Mon amour m'a emporté ...": the participle agrees with "m'" and gives a gendered form; kept "Mon amour a voulu m'emporter dans ses bras ...". Refused 2026-11-12.
- `BluesAlreadySatisfied` (French): "est déjà suffisamment diverti": gendered participle with `{0}`; kept "n'a plus besoin de se divertir". Refused 2026-11-12.
- `BluesActiveHoursDesc` (French): "Cliquer avec le bouton gauche sur une heure ...": "Clic gauche ..." is already clear and does not address the player; matter of taste. Refused 2026-11-12.
- `BluesExtendBuildingsDesc` (English): "that no JoyGiverDef explicitly lists": "which no JoyGiverDef lists by name" is already correct; matter of taste. Refused 2026-11-12.
- `BluesCarryLoverTo` (French): keep two placeholders `{0}` and `{1}`: `{1}` was an English objective pronoun misused as a possessive ("off him feet"); the token is gone from English and French (one argument). Refused 2026-11-12.
