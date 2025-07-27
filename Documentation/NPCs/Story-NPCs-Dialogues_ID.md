# Data Dialog NPC Cerita (Bahasa Indonesia)

Dokumen ini berisi data dialog lengkap untuk semua NPC penting dalam cerita game folklore Trenggalek.

## Ikhtisar NPC Cerita

### Karakter Cerita Utama
- **Ki Ageng Sinawang** - Mentor dan pemimpin padepokan
- **Raden Ayu Saraswati** - Ibu Menak Sopal
- **Mbok Randa Krandon** - Pemilik gajah putih, antagonis utama
- **Buaya Putih** - Roh buaya putih (bos mistis)

### Karakter Cerita Pendukung
- **Murid Padepokan 1-3** - Teman-teman belajar
- **Warga Krandon 1-5** - Warga yang mengejar
- **Pemandu Jalan** - Penunjuk jalan ke Desa Krandon
- **Warga Haus 1-4** - Warga yang kehausan di sumur

---

## Ki Ageng Sinawang (Guru Spiritual)

**NPC ID:** `ki_ageng_sinawang`
**Peran:** Pemimpin padepokan, guru spiritual Menak Sopal

### Entri Dialog

#### Salam Awal (Pra-Cerita)
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Ah, Menak Sopal. Aku merasakan hatimu gelisah hari ini. Angin bercerita tentang perubahan yang akan datang ke tanah kita."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
chapterReference: "Chapter 1: The Peaceful Morning"
storyProgression: "1.1 Morning at the Padepokan"
flagReference: "story_started"
```

#### Fase Cerita 1 - Setelah Penemuan Krisis Air
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Penderitaan rakyat kita memberatkan hatimu, anakku. Terkadang perbuatan mulia yang terbesar memerlukan pengorbanan yang besar pula."
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: ["water_crisis_discovered"]
hasChoices: true
chapterReference: "Chapter 2: The Call to Action"
storyProgression: "1.3 Seeking Guidance"
flagReference: "guru_guidance_received"
choices:
  - choiceText: "Guru, saya ingin membantu mengatasi kekurangan air ini"
    flagsToAdd: ["asked_permission_water_project"]
    questReference: "dam_construction_project"
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Belas kasihanmu menghormati ajaran kita. Pergilah, tapi ingatlah - kebijaksanaan sejati terletak pada pemahaman semua konsekuensi dari tindakan kita."
  - choiceText: "Menurut Guru, apa yang harus saya lakukan?"
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Jawabannya ada dalam dirimu, nak. Dengarkan hatimu, tapi tempa dengan kebijaksanaan. Jalan seorang penolong tidak pernah sederhana."
```

#### Fase Cerita 2 - Bantuan Pembangunan Bendungan
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Ajaklah beberapa murid kita untuk membantumu. Tangan-tangan muda yang bekerja bersama dapat memindahkan gunung - atau dalam hal ini, membangun sungai."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: ["dam_construction_started"]
chapterReference: "Chapter 3: Building Hope"
storyProgression: "2.1 Dam Construction Planning"
flagReference: "students_permission_granted"
choices:
  - choiceText: "Terima kasih, Guru. Kebijaksanaan Guru membimbing saya"
    flagsToAdd: ["students_permission_granted"]
    questToStart: "gather_construction_helpers"
```

#### Fase Cerita 3 - Konsultasi Mistis
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Aku merasakan kekuatan spiritual gelap sedang bekerja. Roh-roh sungai itu kuno dan angkuh. Mereka tidak suka dengan pembangunan yang tidak diundang."
availableTimesOfDay: [Evening, Night]
requiredFlags: ["dam_repeatedly_destroyed"]
chapterReference: "Chapter 4: Mysterious Opposition"
storyProgression: "3.1 Mysterious Destructions"
flagReference: "spiritual_interference_confirmed"
choices:
  - choiceText: "Bagaimana aku bisa menenangkan roh-roh sungai?"
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Roh-roh sering menuntut persembahan atau penghormatan. Carilah komunikasi dulu, nak. Kekerasan haruslah jalan terakhir."
  - choiceText: "Apakah ada bahaya dalam menghadapi roh-roh ini?"
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Semua urusan spiritual mengandung risiko. Tapi niat sucimu mungkin akan melindungimu. Percayalah pada latihanmu."
```

#### Fase Cerita 4 - Dilema Gajah Putih
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Mbok Randa Krandon berhati baik, meski temperamental. Dia akan mengerti jika kamu menjelaskan kebaikan yang lebih besar yang dilayani tindakanmu."
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: ["white_elephant_taken", "mbok_randa_angry"]
chapterReference: "Chapter 8: The Reckoning"
storyProgression: "6.1 Mbok Randa's Discovery"
flagReference: "guru_advice_reconciliation"
choices:
  - choiceText: "Dia sangat marah padaku. Bagaimana aku bisa memperbaiki ini?"
    flagsToAdd: ["guru_advice_reconciliation"]
    response:
      speakerName: "Ki Ageng Sinawang"
      responseText: "Kebenaran yang diucapkan dengan penyesalan tulus dapat menyembuhkan banyak luka. Tunjukkan padanya kebaikan yang datang dari tindakanmu."
```

#### Kesimpulan Cerita - Refleksi Kebijaksanaan
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Kamu telah belajar bahwa bahkan niat mulia pun dapat menyebabkan rasa sakit. Tapi dari rasa sakit ini, pemahaman tumbuh. Desa kini memiliki air, dan kamu memiliki kebijaksanaan."
availableTimesOfDay: [Any]
requiredFlags: ["story_completed", "reconciliation_complete"]
isRepeatable: true
chapterReference: "Chapter 9: Truth, Forgiveness, and Understanding"
storyProgression: "9.2 Story Conclusion"
flagReference: "wisdom_gained"
```

### Dialog Kasual (Waktu Non-cerita)

#### Kebijaksanaan Harian
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Gunung mengajarkan kita kesabaran, sungai mengajarkan kita ketekunan. Pelajaran apa yang akan kamu pilih untuk dipelajari hari ini?"
availableTimesOfDay: [Morning]
isRepeatable: true
```

#### Bimbingan Meditasi
```yaml
speakerName: "Ki Ageng Sinawang"
dialogueText: "Duduklah dengan tenang di bawah pohon beringin tua saat matahari terbenam. Dengarkan apa yang dikatakan angin tentang hari esok."
availableTimesOfDay: [Afternoon]
isRepeatable: true
```

---

## Raden Ayu Saraswati (Ibu)

**NPC ID:** `raden_ayu_saraswati`
**Peran:** Ibu Menak Sopal, sosok ibu yang mendukung

### Entri Dialog

#### Berkah Pagi
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Selamat pagi, anakku tersayang. Ibu bermimpi tentang air yang mengalir tadi malam. Mungkin itu pertanda keberuntungan akan datang."
availableTimesOfDay: [Morning]
requiredFlags: []
isRepeatable: true
chapterReference: "Chapter 1: The Peaceful Morning"
storyProgression: "1.1 Morning at the Padepokan"
```

#### Fase Cerita - Kekhawatiran Seorang Ibu
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Ibu khawatir dengan proyek bendunganmu ini, nak. Roh-roh sungai tidak boleh dianggap enteng."
availableTimesOfDay: [Evening]
requiredFlags: ["dam_construction_started"]
chapterReference: "Chapter 3: Building Hope"
storyProgression: "2.1 Dam Construction Planning"
flagReference: "dam_construction_started"
choices:
  - choiceText: "Jangan khawatir, Ibu. Saya akan berhati-hati"
    response:
      speakerName: "Raden Ayu Saraswati"
      responseText: "Ayahmu dulu memiliki semangat yang sama. Ingatlah saja, keberanian tanpa kebijaksanaan adalah kecerobohan."
  - choiceText: "Apakah Ibu melihat pertanda tentang sungai?"
    response:
      speakerName: "Raden Ayu Saraswati"
      responseText: "Burung-burung gelisah di dekat air. Dan pelita kelahiranmu berkedip-kedip tadi malam - ada sesuatu yang bergerak di alam spiritual."
```

#### Fase Cerita - Dukungan Krisis
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Ketika masalah tampak tak terkendali, ingatlah bahwa setiap badai akan berlalu. Hati baikmu akan menemukan jalan melewati ini."
availableTimesOfDay: [Any]
requiredFlags: ["dam_repeatedly_destroyed"]
chapterReference: "Chapter 4: Mysterious Opposition"
storyProgression: "3.1 Mysterious Destructions"
flagReference: "dam_repeatedly_destroyed"
```

#### Fase Cerita - Konflik Mbok Randa
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Mbok Randa ada di sini, dan dia cukup kesal. Tapi Ibu merasakan kemarahannya berasal dari luka hati, bukan kebencian. Bersikaplah lembut padanya."
availableTimesOfDay: [Any]
requiredFlags: ["mbok_randa_visits_padepokan"]
chapterReference: "Chapter 9: Truth, Forgiveness, and Understanding"
storyProgression: "7.2 Mbok Randa's Pursuit to Padepokan"
flagReference: "confronted_at_padepokan"
choices:
  - choiceText: "Apa yang harus saya katakan padanya?"
    response:
      speakerName: "Raden Ayu Saraswati"
      responseText: "Berbicaralah dari hatimu. Ceritakan mengapa kamu melakukan apa yang kamu lakukan. Terkadang pemahaman adalah semua yang dibutuhkan seseorang."
```

#### Kesimpulan Cerita - Ibu yang Bangga
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Anakku telah menjadi pria sejati hari ini. Bukan karena dia memecahkan masalah, tapi karena dia belajar menghadapi konsekuensi dari pilihannya."
availableTimesOfDay: [Any]
requiredFlags: ["story_completed"]
isRepeatable: true
chapterReference: "Chapter 9: Truth, Forgiveness, and Understanding"
storyProgression: "9.2 Story Conclusion"
flagReference: "story_completed"
```

#### Setelah Cerita - Penyelamatan Buaya Putih
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Jangan khawatir tentang apa yang terjadi di sungai. Buaya putih itu kuno dan bijaksana - dia tidak akan membiarkan bahaya menimpa seseorang yang berhati suci."
availableTimesOfDay: [Any]
requiredFlags: ["rescued_by_crocodile"]
chapterReference: "Chapter 8: The Reckoning"
storyProgression: "6.3 The River Rescue"
flagReference: "rescued_by_crocodile"
```

### Dialog Kasual

#### Perhatian Sore
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Sudah cukupkah kamu makan hari ini? Seorang ibu selalu khawatir anaknya tidak makan dengan baik."
availableTimesOfDay: [Evening]
isRepeatable: true
```

#### Kebijaksanaan Herbal
```yaml
speakerName: "Raden Ayu Saraswati"
dialogueText: "Ibu sedang menyiapkan ramuan penyembuhan untuk desa. Serai di tepi sungai tumbuh sangat baik musim ini."
availableTimesOfDay: [Afternoon]
isRepeatable: true
```

---

## Mbok Randa Krandon (Antagonis)

**NPC ID:** `mbok_randa_krandon`
**Peran:** Pemilik gajah putih, mewakili konflik dan pemahaman akhir

### Entri Dialog

#### Pertemuan Pertama - Sambutan Mencurigakan
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "Pemuda dari padepokan? Apa yang membawamu sejauh ini dari rumah, nak?"
availableTimesOfDay: [Any]
requiredFlags: ["arrived_desa_krandon"]
chapterReference: "Chapter 6: The Sacred Quest"
storyProgression: "4.3 Meeting Mbok Randa Krandon"
flagReference: "arrived_desa_krandon"
choices:
  - choiceText: "Saya datang mencari gajah putih Mbok"
    flagsToAdd: ["requested_elephant_directly"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Gajahku? Itu permintaan yang aneh. Mengapa murid padepokan membutuhkan gajah berhargaku?"
  - choiceText: "Saya datang membawa salam dari Ki Ageng Sinawang"
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Ah, Ki Ageng! Aku mengenalnya ketika dia masih guru muda. Orang baik. Apa yang dia butuhkan?"
```

#### Negosiasi
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "Kamu ingin meminjam gajah putihku? Selama tiga hari? Itu cukup tidak biasa... tapi Ki Ageng menjaminmu."
availableTimesOfDay: [Any]
requiredFlags: ["explained_water_crisis"]
chapterReference: "Chapter 6: The Sacred Quest"
storyProgression: "4.3 Meeting Mbok Randa Krandon"
flagReference: "promised_safe_return"
choices:
  - choiceText: "Saya berjanji akan mengembalikannya dengan selamat"
    flagsToAdd: ["promised_safe_return"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Baiklah. Tapi jika ada bahaya yang menimpanya, padepokanmu akan bertanggung jawab. Tiga hari, tidak lebih."
  - choiceText: "Bagaimana jika sesuatu terjadi pada gajah itu?"
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Maka kamu akan membuat musuh yang sangat kuat. Tapi... aku percaya pada penilaian Ki Ageng terhadap karakter."
```

#### Penemuan Pengkhianatan
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "KAMU! Kamu menipu aku! Di mana gajah putihku? Apa yang telah kamu lakukan padanya?"
availableTimesOfDay: [Any]
requiredFlags: ["elephant_sacrifice_revealed"]
isImportantDialogue: true
chapterReference: "Chapter 8: The Reckoning"
storyProgression: "6.1 Mbok Randa's Discovery"
flagReference: "mbok_randa_angry"
choices:
  - choiceText: "Saya bisa menjelaskan semuanya..."
    flagsToAdd: ["attempted_explanation"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Menjelaskan? MENJELASKAN?! Kamu mengambil gajah kesayanganku dan... dan... Aku tidak seharusnya mempercayai murid padepokan!"
  - choiceText: "Ini demi kebaikan banyak orang"
    flagsToAdd: ["justified_actions"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Kebaikan banyak orang? Bagaimana dengan kehilanganKU? Bagaimana dengan rasa sakitKU? Tangkap dia! Jangan biarkan dia kabur!"
```

#### Di Padepokan - Kebenaran Terungkap
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "Ki Ageng, muridmu telah mengkhianati kepercayaanku! Dia mengambil gajahku dengan dalih palsu!"
availableTimesOfDay: [Any]
requiredFlags: ["confronted_at_padepokan"]
chapterReference: "Chapter 9: Truth, Forgiveness, and Understanding"
storyProgression: "7.2 Mbok Randa's Pursuit to Padepokan"
flagReference: "full_truth_explained"
choices:
  - choiceText: "Tolong biarkan saya menjelaskan seluruh kebenaran"
    targetDialogueIndex: 1  # Lanjut ke penjelasan
```

#### Pemahaman dan Pengampunan
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "Jadi... pengorbanan gajahku membawa air ke desamu? Dan menyelamatkan banyak orang dari penderitaan?"
availableTimesOfDay: [Any]
requiredFlags: ["full_truth_explained"]
chapterReference: "Chapter 9: Truth, Forgiveness, and Understanding"
storyProgression: "8.1 Full Truth Revelation"
flagReference: "sincere_apology_given"
choices:
  - choiceText: "Ya, Mbok. Dan saya benar-benar minta maaf karena menipu Mbok"
    flagsToAdd: ["sincere_apology_given"]
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Penyesalanmu tampak tulus. Dan jika pengorbanan gajahku membantu banyak orang... maka mungkin kematiannya memiliki tujuan mulia."
  - choiceText: "Akankah Mbok memaafkan saya?"
    response:
      speakerName: "Mbok Randa Krandon"
      responseText: "Pengampunan lebih mudah ketika pemahaman datang lebih dulu. Aku memaafkanmu, nak. Tapi rasa sakit kehilangan tetap ada."
```

#### Rekonsiliasi Selesai
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "Jika tanah ini makmur dari pengorbanan gajahku, maka biarlah disebut 'Teranging Galih' - terangnya pemahaman."
availableTimesOfDay: [Any]
requiredFlags: ["reconciliation_complete"]
flagsToAdd: ["teranging_galih_named"]
isImportantDialogue: true
chapterReference: "Chapter 9: Truth, Forgiveness, and Understanding"
storyProgression: "9.1 Naming the Land"
flagReference: "land_naming_complete"
```

### Dialog Pasca-Cerita

#### Refleksi Damai
```yaml
speakerName: "Mbok Randa Krandon"
dialogueText: "Aku masih merindukan gajahku, tapi aku melihat anak-anak bermain di tepi sungai lagi. Itu membawa sedikit penghiburan bagi hati tua ini."
availableTimesOfDay: [Any]
requiredFlags: ["story_completed"]
isRepeatable: true
```

---

## Buaya Putih (Roh Buaya Putih)

**NPC ID:** `buaya_putih_spirit`
**Peran:** Penjaga mistis, mewakili tuntutan alam dan kerjasama akhir

### Entri Dialog

#### Kontak Spiritual Pertama
```yaml
speakerName: "Buaya Putih"
dialogueText: "Siapa yang berani mengganggu air kuno tanpa meminta izin dari penjaganya?"
availableTimesOfDay: [Any]
requiredFlags: ["spiritual_vision_active"]
isImportantDialogue: true
chapterReference: "Chapter 5: Communion with Spirits"
storyProgression: "3.2 Spiritual Revelation"
flagReference: "river_spirit_encountered"
choices:
  - choiceText: "Saya Menak Sopal. Saya berusaha membantu rakyat saya"
    response:
      speakerName: "Buaya Putih"
      responseText: "Membantu? Dengan membangun bendungan di sungaiKU? Niatmu mungkin murni, tapi caramu menunjukkan ketidakhormatan."
  - choiceText: "Roh agung, saya tidak bermaksud menyinggung"
    flagsToAdd: ["showed_respect_to_spirit"]
    response:
      speakerName: "Buaya Putih"
      responseText: "Penghormatan ditunjukkan melalui tindakan, bukan kata-kata. Kamu membangun tanpa bertanya, mengambil tanpa memberi."
```

#### Tuntutan
```yaml
speakerName: "Buaya Putih"
dialogueText: "Jika kamu ingin bendunganmu berdiri, kamu harus menawarkan persembahan yang layak. Bawakan aku kepala gajah putih, dan aku akan menghentikan kerusakanku."
availableTimesOfDay: [Any]
requiredFlags: ["first_contact_complete"]
isImportantDialogue: true
chapterReference: "Chapter 5: Communion with Spirits"
storyProgression: "3.2 Spiritual Revelation"
flagReference: "accepted_spirit_demand"
choices:
  - choiceText: "Mengapa Anda memerlukan pengorbanan seperti itu?"
    response:
      speakerName: "Buaya Putih"
      responseText: "Gajah putih itu suci, seperti diriku. Hanya persembahan suci yang dapat menyeimbangkan tatanan kosmis yang telah kamu ganggu."
  - choiceText: "Pasti ada cara lain"
    response:
      speakerName: "Buaya Putih"
      responseText: "Tidak ada cara lain. Hukum kuno menuntut keseimbangan. Ganggu air, bayar harganya."
  - choiceText: "Saya akan mencari gajah putih ini"
    flagsToAdd: ["accepted_spirit_demand"]
    questToStart: "find_white_elephant"
```

#### Setelah Pengorbanan
```yaml
speakerName: "Buaya Putih"
dialogueText: "Persembahan itu dapat diterima. Bendunganmu akan berdiri, dan air akan mengalir sesuai kebutuhan. Keseimbangan telah dipulihkan."
availableTimesOfDay: [Any]
requiredFlags: ["elephant_sacrifice_complete"]
isImportantDialogue: true
flagsToAdd: ["spirit_pact_complete"]
chapterReference: "Chapter 7: The Terrible Choice"
storyProgression: "5.1 The Sacrifice"
flagReference: "spirit_pact_complete"
```

#### Penyelamatan
```yaml
speakerName: "Buaya Putih"
dialogueText: "Anak muda yang menghormati cara-cara kuno, aku tidak akan membiarkanmu tenggelam. Hatimu yang murni telah mendapat perlindunganku."
availableTimesOfDay: [Any]
requiredFlags: ["drowning_in_river"]
isImportantDialogue: true
flagsToAdd: ["rescued_by_crocodile"]
chapterReference: "Chapter 8: The Reckoning"
storyProgression: "6.3 The River Rescue"
flagReference: "spirit_protection_granted"
```

#### Pemahaman Akhir
```yaml
speakerName: "Buaya Putih"
dialogueText: "Ingatlah pelajaran ini: Alam memberi dengan bebas kepada mereka yang mendekati dengan hormat, tapi menuntut bayaran dari mereka yang mengambil tanpa meminta."
availableTimesOfDay: [Any]
requiredFlags: ["rescued_by_crocodile"]
isImportantDialogue: true
chapterReference: "Chapter 8: The Reckoning"
storyProgression: "6.3 The River Rescue"
flagReference: "wisdom_gained"
```

---

## Karakter Pendukung

### Murid Padepokan (Murid-murid)

#### Murid Padepokan 1
**NPC ID:** `murid_padepokan_1`

```yaml
speakerName: "Andi (Murid Padepokan)"
dialogueText: "Menak Sopal! Aku dengar tentang proyek bendunganmu. Bisakah kami membantu? Kami kuat dan bersemangat melayani masyarakat!"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: ["dam_construction_started"]
chapterReference: "Chapter 3: Building Hope"
storyProgression: "2.1 Dam Construction Planning"
flagReference: "student_helpers_recruited"
choices:
  - choiceText: "Ya, saya butuh bantuan mengangkut batu dan kayu"
    flagsToAdd: ["student_helpers_recruited"]
    questToStart: "gather_construction_materials"
```

#### Murid Padepokan 2
**NPC ID:** `murid_padepokan_2`

```yaml
speakerName: "Budi (Murid Padepokan)"
dialogueText: "Bendungan ini terus rusak! Ada sesuatu yang tidak wajar tentang ini. Aku melihat riak aneh di air saat bendungan runtuh."
availableTimesOfDay: [Any]
requiredFlags: ["dam_repeatedly_destroyed"]
chapterReference: "Chapter 4: Mysterious Opposition"
storyProgression: "3.1 Mysterious Destructions"
flagReference: "spiritual_interference_confirmed"
```

#### Murid Padepokan 3
**NPC ID:** `murid_padepokan_3`

```yaml
speakerName: "Candra (Murid Padepokan)"
dialogueText: "Kakak Menak, kami percaya pada visimu. Jika kakak bilang bendungan ini akan membantu orang, maka kami akan bekerja siang malam untuk membangunnya!"
availableTimesOfDay: [Any]
requiredFlags: ["students_permission_granted"]
chapterReference: "Chapter 3: Building Hope"
storyProgression: "2.1 Dam Construction Planning"
```

### Warga Haus (Warga yang Kehausan)

#### Warga Haus 1
**NPC ID:** `warga_haus_1`

```yaml
speakerName: "Pak Darmo"
dialogueText: "Tolong, anak muda! Anak-anakku sudah berhari-hari tidak mendapat air bersih! Sumur ini hampir kering!"
availableTimesOfDay: [Any]
requiredFlags: []
flagsToAdd: ["water_crisis_discovered"]
isImportantDialogue: true
chapterReference: "Chapter 2: The Call to Action"
storyProgression: "1.2 The Urgent Call"
flagReference: "water_crisis_discovered"
```

#### Warga Haus 2
**NPC ID:** `warga_haus_2`

```yaml
speakerName: "Bu Siti"
dialogueText: "Kami sudah bertengkar karena tetes-tetes terakhir ini! Ini tidak benar! Kami tetangga, bukan musuh!"
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
chapterReference: "Chapter 2: The Call to Action"
storyProgression: "1.2 The Urgent Call"
```

### Warga Krandon (Warga yang Mengejar)

#### Warga Krandon 1
**NPC ID:** `warga_krandon_1`

```yaml
speakerName: "Pak Gunawan"
dialogueText: "Itu dia! Itu pemuda yang mencuri gajah Mbok Randa! Jangan biarkan dia kabur!"
availableTimesOfDay: [Any]
requiredFlags: ["chase_sequence_active"]
isImportantDialogue: true
chapterReference: "Chapter 8: The Reckoning"
storyProgression: "6.2 The Chase"
flagReference: "chase_sequence_active"
```

### Pemandu Jalan (Penunjuk Jalan)

**NPC ID:** `pemandu_jalan`

```yaml
speakerName: "Joko (Penunjuk Jalan Desa)"
dialogueText: "Aku tahu jalan ke Desa Krandon, Kakak. Perjalanan dua hari melewati hutan. Aku akan membimbing Kakak dengan aman ke sana."
availableTimesOfDay: [Any]
requiredFlags: ["seeking_white_elephant"]
chapterReference: "Chapter 6: The Sacred Quest"
storyProgression: "4.2 Journey to Desa Krandon"
flagReference: "guide_hired"
choices:
  - choiceText: "Tolong tunjukkan jalan ke rumah Mbok Randa"
    flagsToAdd: ["guide_hired"]
    questToStart: "journey_to_krandon"
```

---

## Catatan Implementasi

### Referensi Silang Dokumen
Dialog ini terintegrasi dengan:
- **Story Progression:** Setiap dialog merujuk pada fase cerita spesifik
- **Chapter Progression:** Referensi bab dan durasi yang tepat
- **Flag System:** Dependency flag yang akurat dan konsisten
- **Quest System:** Integrasi quest yang sesuai dengan QuestData

### Dependensi Flag
Semua dialog dirancang untuk bekerja dengan sistem flag yang ada:
- Flag memicu progres cerita
- Jalur dialog multipel berdasarkan pilihan pemain
- Dialog kasual yang dapat diulang untuk imersi

### Integrasi Waktu Harian
- **Pagi:** Salam formal, diskusi perencanaan
- **Siang:** Percakapan terkait pekerjaan
- **Sore:** Refleksi, berbagi kebijaksanaan
- **Malam:** Pertemuan mistis/spiritual

### Konsekuensi Pilihan
- Pilihan mempengaruhi keadaan flag dan progres cerita
- Cabang dialog multipel untuk pendekatan pemain yang berbeda
- Beberapa pilihan mengunci/membuka opsi percakapan masa depan

### Integrasi Quest
- Dialog terintegrasi dengan sistem QuestData Anda
- NPC dapat memulai, menyelesaikan, atau memperbarui objektif quest
- Quest kritis cerita dipicu oleh dialog kunci

### Keaslian Budaya
- Penggunaan Bahasa Indonesia yang sopan dan sesuai konteks
- Penghormatan kepada orang tua dan guru
- Nilai-nilai tradisional Jawa dalam dialog
- Konsep spiritual dan mistis yang autentik