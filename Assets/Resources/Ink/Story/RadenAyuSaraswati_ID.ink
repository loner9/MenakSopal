INCLUDE Globals.ink

-> start

=== start ===
{ hasFlag("story_completed"):
    Raden Ayu Saraswati: Anakku telah menjadi pria sejati hari ini. Bukan karena dia memecahkan masalah, tapi karena dia belajar menghadapi konsekuensi dari pilihannya.
    -> END
}
{ hasFlag("dam_construction_started"):
    Raden Ayu Saraswati: Ibu khawatir dengan proyek bendunganmu ini, nak. Roh-roh sungai tidak boleh dianggap enteng.
    + [Jangan khawatir, Ibu. Saya akan berhati-hati]
        Raden Ayu Saraswati: Ayahmu dulu memiliki semangat yang sama. Ingatlah saja, keberanian tanpa kebijaksanaan adalah kecerobohan.
        -> END
    + [Apakah Ibu melihat pertanda tentang sungai?]
        Raden Ayu Saraswati: Burung-burung gelisah di dekat air. Dan pelita kelahiranmu berkedip-kedip tadi malam - ada sesuatu yang bergerak di alam spiritual.
        -> END
}

Raden Ayu Saraswati: Selamat pagi, menak sopal anakku. Ibu bermimpi tentang air yang mengalir tadi malam. Mungkin itu pertanda keberuntungan akan datang.
-> END
