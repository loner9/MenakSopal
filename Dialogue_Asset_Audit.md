# Audit & Diagnostic Report: Dialogue Assets (`DialogueData`)

This document summarizes the audit across all 28 dialogue `.asset` files in the project (`Assets/Resources/Dialogues/`).

---

## 🚨 Summary of Critical Issues Found

| Asset File | Entry | Issue Type | Impact |
| :--- | :--- | :--- | :--- |
| **[BuayaPutih_ID.asset](file:///e:/Garapan/UnityProject/MenakSopal/Assets/Resources/Dialogues/Story/BuayaPutih_ID.asset)** | Entry 1 | **Infinite Loop** (`continueToNext: true`, `nextDialogueIndex: -1`) | Clicking *"Menerima"* loops back to Entry 4 $\rightarrow$ Entry 1 repeatedly. |
| **[BuayaPutih_ID.asset](file:///e:/Garapan/UnityProject/MenakSopal/Assets/Resources/Dialogues/Story/BuayaPutih_ID.asset)** | Entry 5 | **UI Freeze / Missing Buttons** (`hasChoices: true`, `choices: []`) | Dialogue panel opens with no choices and no continue/end buttons. |
| **[JokoGuide_ID.asset](file:///e:/Garapan/UnityProject/MenakSopal/Assets/Resources/Dialogues/Story/JokoGuide_ID.asset)** | Entry 3 | **UI Freeze / Missing Buttons** (`hasChoices: true`, `choices: []`) | *"Ayo nak, saatnya kita bangun dam ini!"* freezes the UI with no buttons. |
| **[MbokRandaKrandon_ID.asset](file:///e:/Garapan/UnityProject/MenakSopal/Assets/Resources/Dialogues/Story/MbokRandaKrandon_ID.asset)** | Entry 3 & 5 | **Blank Choice Buttons** (`choiceText: ""` with `hasChoices: true`) | Displays empty, blank button boxes player must click to proceed. |
| **[AndiStudent_ID.asset](file:///e:/Garapan/UnityProject/MenakSopal/Assets/Resources/Dialogues/Story/AndiStudent_ID.asset)** | Entry 0 | **Unintended Spillover** (`continueToNext: true`, `nextDialogueIndex: -1`) | After recruiting Andi, dialogue does not close cleanly; spills over into next available entry. |
| **[PakTani_ID.asset](file:///e:/Garapan/UnityProject/MenakSopal/Assets/Resources/Dialogues/Story/PakTani_ID.asset)** | Entry 0 | **Unintended Spillover** (`continueToNext: true`, `nextDialogueIndex: -1`) | Asking for farming advice in the morning automatically forces Entry 1 (*"Musim panen..."*) right after. |
| **[RadenAyuSaraswati_ID.asset](file:///e:/Garapan/UnityProject/MenakSopal/Assets/Resources/Dialogues/Story/RadenAyuSaraswati_ID.asset)** | Entry 1 | **Unintended Spillover** (`continueToNext: true`, `nextDialogueIndex: -1`) | Mother's response does not explicitly terminate dialogue. |
| **`*_EN.asset` Files** | All | **Empty Dialogue Entries** (`dialogueEntries: []`) | English story dialogues for Buaya Putih, Ki Ageng, Mbok Randa, and Raden Ayu will immediately close/do nothing. |

---

## 🔍 Detailed Diagnostics & Solutions

### 1. `BuayaPutih_ID.asset`
* **Entry 1 (*"Jika kamu ingin bendunganmu berdiri..."*)**:
  * **Choice 0 ("Menerima")**:
    * Currently has `continueToNext: 1` and `nextDialogueIndex: -1`.
    * **Fix**: Change `continueToNext` to **`0` (`false`)**. This ensures that after Menak Sopal says *"Baiklah, saya akan mencari gajah putih ini"*, the dialogue concludes and frees the player to begin the quest.
* **Entry 5 (*"Begitu rupanya. Kali ini aku sedang dalam suasana baik..."*)**:
  * Currently has `hasChoices: 1` with `choices: []`.
  * **Fix**: Change `hasChoices` to **`0` (`false`)**.

---

### 2. `JokoGuide_ID.asset`
* **Entry 3 (*"Ayo nak, saatnya kita bangun dam ini!"*)**:
  * Has `hasChoices: 1` with empty `choices: []`.
  * **Fix**: Change `hasChoices` to **`0` (`false`)**.
* **Clean-up on Entries 5, 7, and 8**:
  * Entries 5, 7, and 8 have `hasChoices: 0`, but have leftover dummy `choices` blocks copied from previous entries. While inactive, clearing them out avoids clutter in the Inspector.

---

### 3. `MbokRandaKrandon_ID.asset`
* **Entry 3 (*"Baiklah, karena aku kenal baik dengan Ki Ageng..."*)**:
  * Has `hasChoices: 1` with a blank choice (empty `choiceText: ""`).
  * **Fix**: Change `hasChoices` to **`0` (`false`)**.
* **Entry 5 (*"Sudah lebih dari satu bulan berlalu..."*)**:
  * Has `hasChoices: 1` with a blank choice (empty `choiceText: ""`).
  * **Fix**: Change `hasChoices` to **`0` (`false`)**.
* **Entry 4 (*"Sebenarnya aku ingin ikut, tapi ada urusan..."*)**:
  * Set `isRepeatable` to **`1` (`true`)** so she repeats her idle farewell if spoken to again before she gets angry.

---

### 4. `AndiStudent_ID.asset`
* **Entry 0 (*"Menak Sopal! Aku dengar tentang rencana pembangungan..."*)**:
  * **Choice 0 ("Tentu")**:
    * Currently has `continueToNext: 1` and `nextDialogueIndex: -1`.
    * **Fix**: Change `continueToNext` to **`0` (`false`)**.

---

### 5. `PakTani_ID.asset`
* **Entry 0 (*"Selamat pagi, anak muda! Embun pagi ini..."*)**:
  * **Choice 0 ("Bisakah Pak mengajarkan saya bertani padi?")**:
    * Currently has `continueToNext: 1` and `nextDialogueIndex: -1`.
    * **Fix**: Change `continueToNext` to **`0` (`false`)** so the morning greeting doesn't automatically bleed into Entry 1 ("Musim panen sudah tiba!").

---

### 6. `RadenAyuSaraswati_ID.asset`
* **Entry 1 (*"Ibu khawatir dengan proyek bendunganmu ini, nak..."*)**:
  * **Choice 0 & Choice 1**:
    * Both have `continueToNext: 1` and `nextDialogueIndex: -1`.
    * **Fix**: Change `continueToNext` to **`0` (`false`)** on both choices to cleanly conclude the conversation.

---

### 7. Empty English Assets (`*_EN.asset`)
* `BuayaPutih_EN.asset`
* `KiAgengSinawang_EN.asset`
* `MbokRandaKrandon_EN.asset`
* `RadenAyuSaraswati_EN.asset`
* **Fix**: Populate with English dialogue lines if English language support is active in your build.

---

## 🛠️ Summary of Clean Rules for `DialogueData` Authoring

1. **Terminal Choices (End of Conversation)**:
   * If a choice is the final line/action of the conversation, inside `response`, always set:
     - `continueToNext = false`
     - `nextDialogueIndex = -1`
2. **Branching Choices (Jumping to Specific Entry)**:
   * If jumping to another line after the response:
     - `continueToNext = true`
     - `nextDialogueIndex = <target_index>`
3. **No Choices / Simple Lines**:
   * If an entry has no choices, make sure `hasChoices = false`. Never leave `hasChoices = true` with 0 choices or blank choice text.
