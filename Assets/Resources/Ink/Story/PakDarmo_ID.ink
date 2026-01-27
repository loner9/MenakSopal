INCLUDE Globals.ink

-> start

=== start ===
// This assumes water_crisis_discovered logic is handled by the conversation itself if not already set
{ !hasFlag("water_crisis_discovered"):
    -> discovery
}

Pak Darmo: Selamat pagi nak.
-> END

=== discovery ===
Pak Darmo: Tolong, nak muda! sudah berhari-hari kami tidak mendapat air bersih! Sumur ini hampir kering!
~ addFlag("water_crisis_discovered")
+ [Bertanya tentang sungai]
    ~ addFlag("river_asked")
    Menak Sopal: Saya dengar ada sungai disekitar desa ini pak, mengapa tidak mengambil air dari sana??
    -> main_explanation

=== main_explanation ===
Pak Darmo: Kalau itu, kamipun sudah mencoba nak. Namun air di sungai juga sudah kering beberapa minggu ini. Air dari hulu tidak sampai ke tempat dimana kami bisa mengambil air. Banyak hal hal berbahaya melebihi tempat kami biasa mengambil air nak...
+ [Aku paham]
    Menak Sopal: Seperti itu pak rupanya. Sangat disayangkan, kami dari padepokan sendiri belum bisa banyak membantu juga, karena kamipun memiliki tanggungan sendiri. Terimakasih atas informasinya pak, sebisanya saya akan membantu mengatasi masalah ini!
    ~ completeObjective("water_crisis_discovery", "witness_crisis")
    -> END
