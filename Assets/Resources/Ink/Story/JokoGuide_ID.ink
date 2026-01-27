INCLUDE Globals.ink

-> start

=== start ===
{ hasFlag("seeking_white_elephant"):
    Joko: Aku tahu jalan ke Desa Krandon, anak muda. Aku bisa memberitahumu arah untuk mencapai desa itu!
    -> END
}
{ hasFlag("to_river"):
    Joko: Ayo nak, saatnya kita bangun dam ini!
    -> END
}
{ hasFlag("committed_to_help") && hasFlag("guru_guidance_received"):
    -> quest_offer
}
{ hasFlag("committed_to_help"):
    Joko: Oh, tentu saja nak!. Krisis air ini telah membuat gaduh kehidupan orang orang disini!. Pun ke sungai, air tidak sampai di tempat yang aman bagi kami. Bersama keahlianmu, pastinya membangun dam ini menjadi lebih aman hahaha.
    -> END
}

Joko: Ah, halo nak
-> END

=== quest_offer ===
Joko: Hei nak, kau seperti dalam masalah. Ada hal yang bisa ku bantu?
+ [Meminta bantuan]
    Menak Sopal: Paman Joko, bersediakah engkau untuk membantuku dalam membangun dam untuk membantu krisis air di pemukiman ini?
    ~ startQuest("journey_to_krandon")
    -> quest_accepted

=== quest_accepted ===
Joko: Oh, tentu saja nak!. Krisis air ini telah membuat gaduh kehidupan orang orang disini!. Pun ke sungai, air tidak sampai di tempat yang aman bagi kami. Bersama keahlianmu, pastinya membangun dam ini menjadi lebih aman hahaha.
-> END
