INCLUDE Globals.ink

-> start

=== start ===
Pak Tani: Selamat pagi, anak muda! Embun pagi ini sangat cocok untuk menanam hari ini. Apa kamu ke sini untuk belajar bertani?
+ [Bisakah Pak mengajarkan saya bertani padi?]
    Pak Tani: Padi butuh air, kesabaran, dan rasa hormat pada tanah. Kalau kamu mau bantu di sawah, Pak akan ajari semuanya!
    -> part2

=== part2 ===
Pak Tani: Musim panen sudah tiba! Pak butuh tangan muda yang kuat untuk bantu kumpulkan padi. Mau bantu Pak?
+ [Saya akan bantu panen]
    ~ addFlag("pak_tani_harvest_accepted")
    ~ startQuest("village_rice_harvest")
    -> END
