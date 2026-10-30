# Dependências e áudio

O código deste repositório está sob MIT. Blade & Sorcery: Nomad, ThunderRoad, Unity e Newtonsoft.Json pertencem aos respectivos titulares e mantêm suas próprias licenças.

As DLLs de referência não fazem parte deste repositório nem do pacote gerado. Obtenha referências compatíveis por meios autorizados. A compilação local foi verificada com referências da instalação PC disponível; isso não comprova compatibilidade binária ou execução no Quest.

## Arquivos OGG excluídos

A autorização de redistribuição dos áudios locais não foi confirmada. Por decisão do responsável pelo projeto, todos os arquivos OGG estão ignorados pelo Git e excluídos do pacote padrão. A licença MIT deste repositório não concede direitos sobre esses áudios.

O mod continua com efeitos visuais e físicos sem áudio. A sincronização sonora exige arquivos obtidos legalmente pelo usuário, copiados para `audio/` junto ao mod:

| Arquivo | Uso |
| --- | --- |
| `atomic_charge.ogg` | Carga do modo por tempo |
| `atomic_blast.ogg` | Introdução e impacto; marcador esperado em 1,08 s |
| `atomic_impact.ogg` | Impacto direto do modo de soltura |

Substituições precisam preservar o marcador ou ajustar `AtomicTiming.BlastImpactOffset` e recompilar. `atomic_full.ogg` não é carregado pelo código atual. `-IncludeLocalAudio` é uma opção para empacotamento local explícito; não autoriza publicar gravações de terceiros.
