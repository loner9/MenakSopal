INCLUDE Globals.ink

-> start

=== start ===
{ hasFlag("to_river"):
    Bayu: Saatnya untuk membangun dam ini kawanku!
    -> END
}
{ hasFlag("committed_to_help") && hasFlag("guru_guidance_received"):
    Bayu: Membangun dam untuk membendung air?. Tentu saja aku akan membantu Menak Sopal!. Lagipula bertani akhir akhir ini dengan keadaan seperti ini cukup menyulitkan bagiku!
    -> END
}

Bayu: Hey, kamu pasti Menak Sopal. Aku Bayu, petani yang baru saja datang di pemukiman ini. Senang berkenalan denganmu!
-> END
