INCLUDE Globals.ink

-> start

=== start ===
{ hasFlag("drowning_in_river"):
    -> drowning_rescue
}
{ hasFlag("elephant_sacrifice_complete"):
    -> pact_completed
}
{ hasFlag("spiritual_vision_active"):
    -> initial_meeting
}
-> END

=== drowning_rescue ===
Buaya Putih: Anak muda yang menghormati cara-cara kuno, aku tidak akan membiarkanmu tenggelam. Hatimu yang murni telah mendapat perlindunganku.
~ addFlag("rescued_by_crocodile")
-> END

=== pact_completed ===
Buaya Putih: Permintaanku telah terpenuhi. Bendunganmu akan berdiri, dan aku tak akan menggagu lagi.
~ addFlag("spirit_pact_complete")
-> END

=== initial_meeting ===
Buaya Putih: Siapa yang berani beraninya mengganggu istirahatku!
+ [Menak Sopal]
    Menak Sopal: Aku, Menak Sopal. Murid dari padepokan Sinawang!. Hendak mencari alasan bendunganku hancur berkali kali
    -> discussion_part_2

=== discussion_part_2 ===
Buaya Putih: Hmm, jadi muara gemuruh gaduh dari tempat istirahatku akhir akhir ini adalah ulahmu!. Berulang kali ku redam namun tak kunjung padam juga, apa kau ingin menantangku nak?!
+ [Menjelaskan]
    Menak Sopal: Tidak, aku tidak berniat menggagumu wahai penunggu tempat ini. Aku hanya ingin membantu hajat banyak orang dengan membangun bendungan ini.
    -> quest_assignment

=== quest_assignment ===
Buaya Putih: Jika kamu ingin bendunganmu berdiri, kamu harus menawarkan persembahan yang layak. Bawakan aku kepala gajah putih, dan aku akan menghentikan kerusakanku.
+ [Saya akan mencari gajah putih ini]
    ~ addFlag("accepted_spirit_demand")
    -> END
