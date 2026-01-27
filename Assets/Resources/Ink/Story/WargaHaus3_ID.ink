INCLUDE Globals.ink

-> start

=== start ===
{ hasFlag("to_river"):
    Karto: Aku siap membantu membangun dam ini Menak Sopal!
    -> END
}
{ hasFlag("committed_to_help"):
    Karto: Membangun dam?, di tengah terik matahari seperti ini?. Tentu saja!!. Dirimu pasti berniat untuk membendung air agar lebih mudah diambil bukan. Tidak perlu banyak bertele tele, ayo kita bangun dam ini!!!
    -> END
}

Karto: Ah, terik sekali. Air semakin menipis tiap hari. Ada perlu apa nak denganku?
+ [Meminta bantuan]
    Menak Sopal: Begini kang, apakah dirimu bersedia untuk membantuku dalam membangun dam ...
    -> help_response

=== help_response ===
Karto: Membangun dam?, di tengah terik matahari seperti ini?. Tentu saja!!. Dirimu pasti berniat untuk membendung air agar lebih mudah diambil bukan. Tidak perlu banyak bertele tele, ayo kita bangun dam ini!!!
-> END
