# Data Dialog NPC Desa (Bahasa Indonesia)

Dokumen ini berisi data dialog lengkap untuk semua NPC gameplay-focused dalam game folklore Trenggalek.

## Ikhtisar NPC Desa

### Petani & Pertanian
- **Pak Tani (Petani)** - Pertanian padi dan bercocok tanam
- **Bu Tani (Istri Petani)** - Nasihat tanaman dan kebijaksanaan lokal
- **Anak Gembala (Bocah Penggembala)** - Ternak dan berita desa

### Pedagang & Perdagangan
- **Pak Pedagang (Saudagar)** - Pedagang barang umum
- **Bu Penjual (Penjaja)** - Makanan dan kebutuhan sehari-hari
- **Pengrajin Kayu (Tukang Kayu)** - Alat dan barang kayu

### Kehidupan Desa
- **Pak Lurah (Kepala Desa)** - Kepemimpinan desa dan masalah
- **Bu Guru (Guru)** - Pendidikan dan anak-anak desa
- **Dukun Kampung (Dukun Desa)** - Pengobatan tradisional dan mistik
- **Pemuda Desa (Pemuda Kampung)** - Energi dan kegiatan desa
- **Nenek Bijak (Sesepuh Bijaksana)** - Cerita tradisional dan kebijaksanaan

### NPC Utilitas
- **Penjaga Gerbang (Penjaga Pintu)** - Keamanan pintu masuk desa
- **Pemburu (Pemburu)** - Pengetahuan hutan dan berburu
- **Nelayan (Nelayan)** - Informasi sungai dan menangkap ikan

---

## Pak Tani (Petani)

**NPC ID:** `pak_tani`
**Peran:** Petani desa, menyediakan quest pertanian dan informasi bercocok tanam padi

### Entri Dialog

#### Salam Pagi
```yaml
speakerName: "Pak Tani"
dialogueText: "Selamat pagi, anak muda! Embun pagi ini sangat cocok untuk menanam hari ini. Apa kamu ke sini untuk belajar bertani?"
availableTimesOfDay: [Morning]
requiredFlags: []
isRepeatable: true
sideQuestReference: "village_rice_harvest"
choices:
  - choiceText: "Bisakah Pak mengajarkan saya bertani padi?"
    response:
      speakerName: "Pak Tani"
      responseText: "Padi butuh air, kesabaran, dan rasa hormat pada tanah. Kalau kamu mau bantu di sawah, Pak akan ajari semuanya!"
  - choiceText: "Saya hanya lewat saja"
    response:
      speakerName: "Pak Tani"
      responseText: "Selamat jalan, nak. Ingat, perut kenyang bikin perjalanan senang!"
```

#### Percakapan Jam Kerja
```yaml
speakerName: "Pak Tani"
dialogueText: "Sawah-sawah ini sudah menghidupi desa kita turun-temurun. Kerja keras dan panen yang baik menjaga semua orang tetap kenyang."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
```

#### Quest: Bantu Panen
```yaml
speakerName: "Pak Tani"
dialogueText: "Musim panen sudah tiba! Pak butuh tangan muda yang kuat untuk bantu kumpulkan padi. Mau bantu Pak?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
sideQuestReference: "village_rice_harvest"
choices:
  - choiceText: "Saya akan bantu panen"
    flagsToAdd: ["pak_tani_harvest_accepted"]
    questToStart: "village_rice_harvest"
    response:
      speakerName: "Pak Tani"
      responseText: "Bagus! Ketemu di sawah sebelah timur. Pak akan ajari cara motong padi yang benar supaya gabahnya tidak rusak."
  - choiceText: "Saya lagi sibuk sekarang"
    response:
      speakerName: "Pak Tani"
      responseText: "Pak mengerti. Kalau berubah pikiran, Pak ada di sawah sampai matahari terbenam."
```

#### Setelah Membantu Quest
```yaml
speakerName: "Pak Tani"
dialogueText: "Kamu berbakat! Bantuanmu bikin panen jadi lebih mudah. Ambil beras ini buat perjalananmu."
availableTimesOfDay: [Any]
requiredFlags: ["village_rice_harvest_complete"]
isRepeatable: true
reputationImpact: "Positive village reputation"
```

#### Periode Krisis Air
```yaml
speakerName: "Pak Tani"
dialogueText: "Kemarau ini parah banget buat tanaman. Kalau terus begini, kita nggak punya beras buat musim tanam berikutnya."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
storyConnection: "Main story water crisis affects farming"
```

#### Pasca Pembangunan Bendungan
```yaml
speakerName: "Pak Tani"
dialogueText: "Alhamdulillah! Air mengalir ke sawah lagi! Pemuda dari padepokan itu benar-benar menyelamatkan mata pencaharian kita."
availableTimesOfDay: [Any]
requiredFlags: ["dam_construction_complete"]
isRepeatable: true
storyConnection: "Positive outcome from main story dam project"
```

---

## Bu Tani (Istri Petani)

**NPC ID:** `bu_tani`
**Peran:** Kebijaksanaan bertani, pengetahuan herbal, resep desa

### Entri Dialog

#### Kebijaksanaan Harian
```yaml
speakerName: "Bu Tani"
dialogueText: "Panen yang baik dimulai dari benih yang baik, tapi diselesaikan dengan masakan yang baik. Mau belajar resep desa?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Tolong ajari saya masak nasi yang benar"
    response:
      speakerName: "Bu Tani"
      responseText: "Rahasianya ada di takaran air dan tahu kapan nasinya 'bernyanyi'. Dengarkan baik-baik suara gelembungnya!"
  - choiceText: "Apakah Bu tahu obat herbal?"
    response:
      speakerName: "Bu Tani"
      responseText: "Serai buat demam, jahe buat sakit perut, sama kunyit buat luka. Alam sudah sediakan semua yang kita butuh!"
```

#### Quest: Kumpulkan Herbal
```yaml
speakerName: "Bu Tani"
dialogueText: "Bu lagi siapkan obat buat anak-anak desa. Bisa bantu Bu kumpulkan ramuan dari hutan?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
sideQuestReference: "gather_healing_herbs"
choices:
  - choiceText: "Herbal apa yang Bu butuhkan?"
    flagsToAdd: ["herb_gathering_quest_available"]
    questToStart: "gather_healing_herbs"
    response:
      speakerName: "Bu Tani"
      responseText: "Bu butuh kunyit, jahe, sama serai. Hati-hati di hutan ya - binatang buas suka jaga tanaman yang bagus!"
```

#### Pelajaran Masak
```yaml
speakerName: "Bu Tani"
dialogueText: "Mau belajar masak gudeg? Itu makanan khas desa kita, turun-temurun dari nenek moyang."
availableTimesOfDay: [Afternoon, Evening]
requiredFlags: ["helped_with_harvest"]
choices:
  - choiceText: "Ya, tolong ajari saya!"
    flagsToAdd: ["cooking_lessons_started"]
    response:
      speakerName: "Bu Tani"
      responseText: "Bagus! Pertama, kita butuh nangka muda, santan, sama gula jawa. Masak itu soal sabar dan cinta."
```

---

## Anak Gembala (Bocah Penggembala)

**NPC ID:** `anak_gembala`
**Peran:** Sumber berita desa, perawatan hewan, pembantu energik

### Entri Dialog

#### Salam Energik
```yaml
speakerName: "Anak Gembala"
dialogueText: "Hai! Aku lagi jaga kambing desa hari ini! Kamu lihat hewan aneh di hutan nggak?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Hewan aneh yang gimana?"
    response:
      speakerName: "Anak Gembala"
      responseText: "Kambing-kambing pada takut akhir-akhir ini. Mereka nggak mau deket-deket sungai! Hewan kan bisa ngerasain hal yang manusia nggak bisa."
  - choiceText: "Bisa ceritain tentang desa ini?"
    response:
      speakerName: "Anak Gembala"
      responseText: "Desa kita paling bagus! Semua orang bantu-bantuan. Tapi akhir-akhir ini orang-orang khawatir soal air..."
```

#### Quest Hewan - Kambing Hilang
```yaml
speakerName: "Anak Gembala"
dialogueText: "Aduh! Salah satu kambingku kabur ke hutan! Warnanya putih ada totol hitam. Bisa bantu cariin?"
availableTimesOfDay: [Afternoon]
requiredFlags: []
sideQuestReference: "find_lost_goat"
choices:
  - choiceText: "Aku akan bantu cari kambingmu"
    flagsToAdd: ["lost_goat_quest_accepted"]
    questToStart: "find_lost_goat"
    response:
      speakerName: "Anak Gembala"
      responseText: "Makasih banyak! Namanya Putih. Dia suka makan tunas bambu muda, jadi coba cek deket rumpun bambu!"
```

#### Sumber Informasi
```yaml
speakerName: "Anak Gembala"
dialogueText: "Dari bukit ini aku bisa lihat semuanya! Mau tau apa yang terjadi di desa hari ini?"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Apa berita terbaru desa?"
    response:
      speakerName: "Anak Gembala"
      responseText: "Pak Lurah tadi pagi rapat sama sesepuh. Mereka kelihatan serius banget. Terus Bu Guru lagi ngajarin lagu baru ke anak-anak!"
```

#### Selama Krisis Air
```yaml
speakerName: "Anak Gembala"
dialogueText: "Kambing-kambing pada haus banget! Biasanya mereka minum di kali, tapi sekarang hampir kering. Aku harus bawa-bawa air buat mereka."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
storyConnection: "Water crisis affects livestock care"
```

---

## Pak Pedagang (Saudagar)

**NPC ID:** `pak_pedagang`
**Peran:** Penjual barang, informasi perdagangan, pemberi quest ekonomi

### Entri Dialog

#### Sambutan Toko
```yaml
speakerName: "Pak Pedagang"
dialogueText: "Selamat datang di toko sederhana Pak! Ada barang dari tiga desa. Bisa Pak bantu cariin apa hari ini?"
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Apa yang Pak jual?"
    response:
      speakerName: "Pak Pedagang"
      responseText: "Ada alat, kain, rempah-rempah, kadang barang ajaib dari dukun keliling. Tergantung pedagang bawa apa!"
  - choiceText: "Pak butuh bantuan usaha?"
    response:
      speakerName: "Pak Pedagang"
      responseText: "Wah, kebetulan! Pak butuh orang buat antar barang ke desa sebelah. Minat cari uang receh?"
```

#### Quest Perdagangan
```yaml
speakerName: "Pak Pedagang"
dialogueText: "Ada kiriman yang harus sampe ke Desa Krandon. Barang berharga, jadi Pak butuh orang yang bisa dipercaya. Minat?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
sideQuestReference: "merchant_delivery_krandon"
choices:
  - choiceText: "Kiriman apa?"
    response:
      speakerName: "Pak Pedagang"
      responseText: "Obat herbal buat dukun mereka. Jalan aman kalau siang, tapi hati-hati sama binatang buas kalau malem."
  - choiceText: "Saya ambil kerja antar barang"
    flagsToAdd: ["delivery_quest_accepted"]
    questToStart: "merchant_delivery_krandon"
    response:
      speakerName: "Pak Pedagang"
      responseText: "Mantap! Ini paketnya. Antar ke Dukun Krandon terus bawa balik bayarannya. Hati-hati di jalan!"
```

#### Informasi Ekonomi
```yaml
speakerName: "Pak Pedagang"
dialogueText: "Dagang susah akhir-akhir ini. Kemarau bikin semua orang kena dampak - petani sedikit yang dijual, orang sedikit yang beli."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
storyConnection: "Economic impact of main story water crisis"
```

#### Barang Khusus (Reward Pasca-Quest)
```yaml
speakerName: "Pak Pedagang"
dialogueText: "Eh, kurir andalan Pak! Ada barang spesial yang baru datang. Mau lihat koleksi premium Pak?"
availableTimesOfDay: [Any]
requiredFlags: ["merchant_delivery_complete"]
isRepeatable: true
reputationImpact: "Better shop prices and selection"
```

---

## Bu Penjual (Penjaja Makanan)

**NPC ID:** `bu_penjual`
**Peran:** Penjual makanan, resep lokal, pengumpul komunitas

### Entri Dialog

#### Sambutan Makanan
```yaml
speakerName: "Bu Penjual"
dialogueText: "Makanan segar! Nasi anget, sambal pedas, sama jajanan manis! Perut kenyang bikin hati senang!"
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Apa makanan andalan Bu?"
    response:
      speakerName: "Bu Penjual"
      responseText: "Nasi gudeg Bu terkenal se-daerah! Resepnya dari nenek buyut nenek Bu."
  - choiceText: "Bisa beli makanan?"
    response:
      speakerName: "Bu Penjual"
      responseText: "Tentu! Anak muda butuh makanan bergizi. Lima uang tembaga dapet makan kenyang."
```

#### Quest Persiapan Festival Komunitas
```yaml
speakerName: "Bu Penjual"
dialogueText: "Festival desa mau datang! Bu butuh bantuan kumpulin bahan buat pesta komunitas. Mau bantu Bu?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
sideQuestReference: "gather_festival_ingredients"
choices:
  - choiceText: "Bahan apa yang Bu butuhkan?"
    flagsToAdd: ["festival_cooking_quest_available"]
    questToStart: "gather_festival_ingredients"
    response:
      speakerName: "Bu Penjual"
      responseText: "Bu butuh ikan dari sungai, sayuran dari sawah, sama rempah dari hutan. Bakal jadi pesta paling enak!"
```

#### Kebijaksanaan Masak
```yaml
speakerName: "Bu Penjual"
dialogueText: "Masak itu kayak hidup - butuh keseimbangan manis, asin, asem, sama pedas. Kebanyakan salah satu aja bisa rusak masakannya."
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
```

---

## Pak Lurah (Kepala Desa)

**NPC ID:** `pak_lurah`
**Peran:** Kepemimpinan desa, pemberi quest utama, pemecah masalah

### Entri Dialog

#### Salam Formal
```yaml
speakerName: "Pak Lurah"
dialogueText: "Selamat datang, anak muda. Saya kepala desa ini. Ada yang bisa saya bantu hari ini?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Saya ingin membantu desa"
    response:
      speakerName: "Pak Lurah"
      responseText: "Niat mulia! Desa maju kalau warganya kerja sama. Selalu ada tugas yang butuh tangan mampu."
  - choiceText: "Tantangan apa yang desa hadapi?"
    response:
      speakerName: "Pak Lurah"
      responseText: "Setiap desa punya masalahnya. Sekarang, kami khawatir musim kering sama menjaga rakyat tetap kenyang dan sehat."
```

#### Kepemimpinan Krisis Air
```yaml
speakerName: "Pak Lurah"
dialogueText: "Situasi air jadi kritis. Saya sudah panggil rapat desa buat bahas solusi. Kita butuh tindakan segera."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
isImportantDialogue: true
storyConnection: "Village leadership response to main story crisis"
choices:
  - choiceText: "Saya mungkin punya solusi"
    flagsToAdd: ["offered_help_to_lurah"]
    response:
      speakerName: "Pak Lurah"
      responseText: "Bantuan apapun sangat dihargai. Kesejahteraan rakyat adalah tanggung jawab terbesar saya."
```

#### Quest Desa Utama
```yaml
speakerName: "Pak Lurah"
dialogueText: "Ada urusan penting buat desa kita. Perampok sudah mengancam jalur dagang kita. Butuh orang berani buat selidiki."
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: ["established_village_reputation"]
sideQuestReference: "investigate_bandit_threat"
choices:
  - choiceText: "Saya akan selidiki masalah perampok"
    flagsToAdd: ["bandit_quest_accepted"]
    questToStart: "investigate_bandit_threat"
    response:
      speakerName: "Pak Lurah"
      responseText: "Saya berharap kamu mau volunteer. Kamu sudah buktiin bisa dipercaya. Hati-hati - perampok ini berbahaya."
```

#### Gratitudo Pasca-Bendungan
```yaml
speakerName: "Pak Lurah"
dialogueText: "Berkat usahamu dengan bendungan, desa kita punya air lagi. Kamu akan selalu diterima di sini, pahlawan muda."
availableTimesOfDay: [Any]
requiredFlags: ["dam_construction_complete"]
isRepeatable: true
storyConnection: "Village leadership gratitude for main story success"
```

---

## Bu Guru (Guru)

**NPC ID:** `bu_guru`
**Peran:** Pendidikan desa, kesejahteraan anak-anak, pelestarian budaya

### Entri Dialog

#### Sambutan Pendidikan
```yaml
speakerName: "Bu Guru"
dialogueText: "Pendidikan adalah cahaya yang menerangi pikiran muda! Kamu ke sini mau belajar, atau mungkin bantu ngajar anak-anak?"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Bisa ajari saya sejarah lokal?"
    response:
      speakerName: "Bu Guru"
      responseText: "Desa kita punya tradisi kaya! Setiap batu dan pohon punya cerita. Mau dengar legenda kuno?"
  - choiceText: "Gimana caranya bantu anak-anak?"
    response:
      speakerName: "Bu Guru"
      responseText: "Anak-anak suka dengar cerita petualangan! Kalau kamu punya cerita buat dibagi, mereka pasti senang."
```

#### Quest: Alat Sekolah
```yaml
speakerName: "Bu Guru"
dialogueText: "Anak-anak butuh alat tulis baru. Bisa bantu Bu kumpulkan daun lontar sama bikin arang buat nulis?"
availableTimesOfDay: [Morning]
requiredFlags: []
sideQuestReference: "gather_school_supplies"
choices:
  - choiceText: "Saya akan bantu kumpulkan alat sekolah"
    flagsToAdd: ["school_supplies_quest_accepted"]
    questToStart: "gather_school_supplies"
    response:
      speakerName: "Bu Guru"
      responseText: "Bagus! Kita butuh daun lontar besar dari hutan sama kayu bakar buat arang. Pendidikan harus terus jalan!"
```

#### Pelestarian Budaya
```yaml
speakerName: "Bu Guru"
dialogueText: "Bu lagi ngajarin anak-anak lagu sama cerita tradisional. Penting banget melestarikan budaya kita buat generasi yang akan datang."
availableTimesOfDay: [Afternoon]
requiredFlags: []
isRepeatable: true
```

#### Kekhawatiran Kesejahteraan Anak
```yaml
speakerName: "Bu Guru"
dialogueText: "Anak-anak khawatir soal kekurangan air. Bu coba bikin mereka tetap optimis, tapi susah kalau orang tua pada stress."
availableTimesOfDay: [Any]
requiredFlags: ["water_crisis_discovered"]
storyConnection: "Educational impact of main story water crisis"
```

---

## Dukun Kampung (Dukun Desa)

**NPC ID:** `dukun_kampung`
**Peran:** Pengobatan tradisional, bimbingan spiritual, quest mistis

### Entri Dialog

#### Salam Mistis
```yaml
speakerName: "Dukun Kampung"
dialogueText: "Roh-roh berbisik tentang kedatanganmu, anak muda. Kamu membawa aura takdir. Apa yang membuatmu mencari cara-cara lama?"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Saya mencari bimbingan spiritual"
    response:
      speakerName: "Dukun Kampung"
      responseText: "Jalan kebijaksanaan dilalui dengan langkah rendah hati. Bermeditasilah di pohon beringin keramat saat bulan purnama."
  - choiceText: "Bisa ajari saya pengobatan tradisional?"
    response:
      speakerName: "Dukun Kampung"
      responseText: "Penyembuhan datang dari memahami keseimbangan tubuh, pikiran, dan jiwa. Tanaman adalah sekutu kita dalam karya suci ini."
```

#### Quest Spiritual
```yaml
speakerName: "Dukun Kampung"
dialogueText: "Saya merasakan gangguan di alam spiritual. Roh-roh sungai gelisah. Mau bantu saya lakukan ritual pembersihan?"
availableTimesOfDay: [Evening, Night]
requiredFlags: ["dam_repeatedly_destroyed"]
sideQuestReference: "river_spirit_cleansing"
storyConnection: "Alternative spiritual approach to main story conflict"
choices:
  - choiceText: "Ritual seperti apa?"
    response:
      speakerName: "Dukun Kampung"
      responseText: "Kita harus kumpulkan herbal suci dan berdoa di kuil sungai. Roh-roh menuntut penghormatan atas wilayah mereka."
  - choiceText: "Saya akan bantu ritual"
    flagsToAdd: ["spiritual_ritual_accepted"]
    questToStart: "river_spirit_cleansing"
    response:
      speakerName: "Dukun Kampung"
      responseText: "Bagus. Bawa bunga putih, dupa, dan hati yang suci. Kita lakukan ritual saat tengah malam."
```

#### Layanan Penyembuhan
```yaml
speakerName: "Dukun Kampung"
dialogueText: "Energimu kelihatan tidak seimbang. Mungkin kamu butuh pembersihan spiritual? Saya bisa siapkan herbal penyembuhan buatmu."
availableTimesOfDay: [Any]
requiredFlags: []
choices:
  - choiceText: "Penyembuhan seperti apa yang bisa diberikan?"
    response:
      speakerName: "Dukun Kampung"
      responseText: "Saya menyembuhkan tubuh dan jiwa. Obat herbal buat penyakit fisik, doa dan ritual buat masalah spiritual."
```

#### Pengetahuan Mistis
```yaml
speakerName: "Dukun Kampung"
dialogueText: "Roh-roh tua ingat ketika tanah ini masih muda. Mereka bicara tentang gajah putih agung dan buaya bijaksana. Apakah visi ini ada artinya buatmu?"
availableTimesOfDay: [Night]
requiredFlags: ["seeking_white_elephant"]
isImportantDialogue: true
storyConnection: "Mystical guidance for main story white elephant quest"
```

---

## Pemuda Desa (Pemuda Kampung)

**NPC ID:** `pemuda_desa`
**Peran:** Pembantu energik, quest fisik, kegiatan desa

### Entri Dialog

#### Salam Antusias
```yaml
speakerName: "Pemuda Desa"
dialogueText: "Eh! Kamu kelihatan kuat! Mau gabung kerja bakti desa? Kita selalu butuh tangan tambahan!"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Kerja apa yang perlu dikerjakan?"
    response:
      speakerName: "Pemuda Desa"
      responseText: "Benerin-benerin, angkat barang berat, bersihin jalan - apa aja yang butuh otot dan semangat! Plus seru kerja bareng!"
  - choiceText: "Saya tertarik membantu"
    response:
      speakerName: "Pemuda Desa"
      responseText: "Mantap! Ketemu di balai desa habis sholat subuh. Kita langsung kasih kerjaan!"
```

#### Quest Konstruksi
```yaml
speakerName: "Pemuda Desa"
dialogueText: "Kita lagi bangun gudang baru buat simpan gabah desa. Butuh orang punya skill kayak kamu buat bantu angkat-angkat berat!"
availableTimesOfDay: [Morning, Afternoon]
requiredFlags: []
sideQuestReference: "village_construction_project"
choices:
  - choiceText: "Saya akan bantu konstruksi"
    flagsToAdd: ["construction_quest_accepted"]
    questToStart: "village_construction_project"
    response:
      speakerName: "Pemuda Desa"
      responseText: "Perfect! Kita harus ambil kayu dari hutan sama angkut batu dari tambang. Kerja berat, tapi pasti seru!"
```

#### Tantangan Atletik
```yaml
speakerName: "Pemuda Desa"
dialogueText: "Eh, kamu kelihatan atletis! Mau lomba lari ke jembatan tua? Yang menang dapet hak pamer!"
availableTimesOfDay: [Afternoon]
requiredFlags: []
choices:
  - choiceText: "Ayo lomba!"
    flagsToAdd: ["racing_challenge_accepted"]
    response:
      speakerName: "Pemuda Desa"
      responseText: "Haha! Gitu dong semangat! Siap? Tiga, dua, satu... GO!"
```

#### Semangat Komunitas
```yaml
speakerName: "Pemuda Desa"
dialogueText: "Desa ini yang besar-besarin aku, jadi aku balikin bantuan sebisanya. Semua orang harus kontribusi buat komunitasnya!"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
```

---

## Nenek Bijak (Sesepuh Bijaksana)

**NPC ID:** `nenek_bijak`
**Peran:** Kebijaksanaan tradisional, cerita rakyat, pengetahuan budaya

### Entri Dialog

#### Salam Bijaksana
```yaml
speakerName: "Nenek Bijak"
dialogueText: "Sini, nak. Mata tua ini sudah lihat banyak musim, telinga ini sudah dengar cerita tak terhitung. Kebijaksanaan apa yang kamu cari?"
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
choices:
  - choiceText: "Ceritakan dongeng tradisional"
    response:
      speakerName: "Nenek Bijak"
      responseText: "Ah, cerita! Mereka bawa kebijaksanaan generasi. Mau dengar tentang saat bulan jatuh cinta sama gunung?"
  - choiceText: "Nasihat apa buat anak muda?"
    response:
      speakerName: "Nenek Bijak"
      responseText: "Dengar lebih banyak daripada bicara, bantu lebih banyak daripada minta, dan ingat setiap akhir juga awal."
```

#### Pengetahuan Rakyat
```yaml
speakerName: "Nenek Bijak"
dialogueText: "Nenek tahu cerita-cerita lama tanah ini - kisah roh, pahlawan berani, dan hewan ajaib. Cerita mana yang panggil hatimu?"
availableTimesOfDay: [Evening, Night]
requiredFlags: []
storyConnection: "Source of cultural knowledge for main story elements"
choices:
  - choiceText: "Ceritakan tentang gajah putih"
    flagsToAdd: ["heard_white_elephant_legend"]
    response:
      speakerName: "Nenek Bijak"
      responseText: "Ah, gajah putih suci! Legenda bilang dia cuma muncul buat orang berhati suci dan butuh besar. Dia berkah sekaligus ujian."
  - choiceText: "Bagaimana dengan roh sungai?"
    response:
      speakerName: "Nenek Bijak"
      responseText: "Roh sungai itu kuno dan angkuh. Mereka ingat saat lembah ini masih hutan belantara. Hormati mereka, mungkin mereka bantu kamu."
```

#### Kebijaksanaan Hidup
```yaml
speakerName: "Nenek Bijak"
dialogueText: "Hidup itu kayak nenun - benang-benang sendiri mungkin lemah, tapi kalau disatuin jadi sesuatu yang kuat dan indah."
availableTimesOfDay: [Any]
requiredFlags: []
isRepeatable: true
```

#### Quest Pengajaran Budaya
```yaml
speakerName: "Nenek Bijak"
dialogueText: "Anak-anak muda harus belajar cara lama sebelum dilupakan. Mau bantu Nenek kumpulin anak-anak buat dongeng?"
availableTimesOfDay: [Evening]
requiredFlags: []
sideQuestReference: "gather_children_storytelling"
choices:
  - choiceText: "Saya akan bantu kumpulkan anak-anak"
    flagsToAdd: ["storytelling_quest_accepted"]
    questToStart: "gather_children_storytelling"
    response:
      speakerName: "Nenek Bijak"
      responseText: "Berkah untukmu, nak. Cerita adalah akar yang bikin budaya tetap hidup. Bawa mereka ke pohon beringin saat matahari terbenam."
```

---

## NPC Desa Pendukung

### Penjaga Gerbang (Penjaga Pintu)

**NPC ID:** `penjaga_gerbang`

```yaml
speakerName: "Penjaga Gerbang"
dialogueText: "Berhenti! Bilang urusan apa di desa kami. Kami terima pelancong jujur tapi waspada sama pembuat onar."
availableTimesOfDay: [Any]
requiredFlags: []
choices:
  - choiceText: "Saya datang dengan damai buat bantu desa"
    flagsToAdd: ["peaceful_intentions_declared"]
    response:
      speakerName: "Penjaga Gerbang"
      responseText: "Bagus. Kita selalu butuh tangan pembantu. Pergi bicara sama Pak Lurah di balai desa."
  - choiceText: "Saya cuma lewat"
    response:
      speakerName: "Penjaga Gerbang"
      responseText: "Selamat jalan. Hati-hati di jalan hutan - binatang buas lagi aktif akhir-akhir ini."
```

### Pemburu (Pemburu)

**NPC ID:** `pemburu`

```yaml
speakerName: "Pemburu"
dialogueText: "Hutan aneh akhir-akhir ini. Binatang gelisah, dan aku lihat jejak yang tidak kukenal. Ada yang bikin mereka takut."
availableTimesOfDay: [Morning, Afternoon, Evening]
requiredFlags: []
storyConnection: "Forest disturbances may relate to spiritual activity"
choices:
  - choiceText: "Jejak aneh seperti apa?"
    response:
      speakerName: "Pemburu"
      responseText: "Jejak besar tidak biasa deket sungai. Bukan dari binatang yang aku tahu. Mungkin dari alam gaib."
  - choiceText: "Bisa ajari saya berburu?"
    response:
      speakerName: "Pemburu"
      responseText: "Berburu soal sabar, hormat, dan ambil sekadarnya aja. Hutan kasih, tapi dia harap rasa terima kasih."
```

### Nelayan (Nelayan)

**NPC ID:** `nelayan`

```yaml
speakerName: "Nelayan"
dialogueText: "Ikan pada aneh akhir-akhir ini - berenang berputar-putar, loncat keluar air. Ada yang ganggu roh sungai."
availableTimesOfDay: [Morning, Evening]
requiredFlags: []
storyConnection: "River disturbances connected to main story spiritual conflict"
choices:
  - choiceText: "Lihat yang aneh di sungai?"
    response:
      speakerName: "Nelayan"
      responseText: "Riak aneh, bayangan yang bergerak melawan arus. Dan kadang, aku sumpah lihat sisik putih berkilau di air dalam."
```

---

## Catatan Implementasi

### Integrasi Quest
Semua NPC desa dirancang untuk berintegrasi dengan sistem QuestData yang ada:
- Beberapa NPC pemberi quest untuk variasi
- Kesulitan progresif dalam tipe quest
- Objektif pembangunan komunitas
- Misi pengumpulan sumber daya

### Dependensi Flag
NPC desa merespons peristiwa cerita utama:
- Krisis air mempengaruhi dialog petani dan penjual makanan
- Penyelesaian bendungan mengubah suasana desa
- Reputasi pemain membuka quest lanjutan

### Kompatibilitas Jadwal
Dialog mempertimbangkan ketersediaan waktu-dalam-hari:
- Petani aktif selama jam kerja
- Pedagang tersedia selama waktu perdagangan
- Sesepuh bijak tersedia untuk cerita sore
- Pertemuan spiritual malam hari

### Pembangunan Komunitas
NPC menciptakan rasa desa yang hidup:
- Hubungan saling terkait antar NPC
- Variasi dialog musiman dan situasional
- Keakraban progresif dengan interaksi berulang
- Acara perayaan komunitas dan festival

### Keaslian Budaya
Semua dialog mempertahankan elemen budaya Indonesia:
- Salam tradisional dan bahasa sopan
- Referensi ke aktivitas dan makanan otentik
- Penggabungan kebijaksanaan dan nilai lokal
- Struktur sosial desa yang autentik

### Sistem Reputasi Terintegrasi
Dialog NPC desa mendukung sistem reputasi:
- **Asing** (0-2 quest) - Hanya interaksi dasar
- **Teman** (3-5 quest) - Dialog yang diperluas, harga lebih baik
- **Anggota Terpercaya** (6-8 quest) - Quest lanjutan, akses khusus
- **Pahlawan Desa** (9+ quest) - Hasil terbaik, rasa hormat komunitas