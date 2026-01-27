INCLUDE Globals.ink

-> start

=== start ===
{ hasFlag("materials_collected"):
    Budi (Murid Padepokan): Sekarang semua bahan meterial terkumpul, mari kita selesaikan bendungan ini, Menak Sopal!
    ~ addFlag("dam_dialog_built")
    -> END
}
{ hasFlag("dam_broken"):
    Budi (Murid Padepokan): Bendungan ini terus rusak! Ada sesuatu yang tidak wajar tentang ini. Aku melihat riak aneh di air saat bendungan runtuh.
    -> END
}
{ hasFlag("committed_to_help") && hasFlag("guru_guidance_received"):
    Budi (Murid Padepokan): Kabar berlalu cepat nak, dan aku disini siap untuk membantu niat baikmu!
    -> END
}

// Default greetings
Budi (Murid Padepokan): Pagi saudara seperguruanku!
-> END
