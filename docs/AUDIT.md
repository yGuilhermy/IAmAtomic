# Preparação para publicação — 29/10/2026

## Resultado

Repositório independente, licença MIT para o código e pacote público sem OGG. Nenhum commit, staging ou push foi executado. Nenhum remoto está configurado.

## Três passadas de segurança

1. Segredos: nomes de arquivos sensíveis e padrões de credenciais verificados na lista pública. Nenhum achado. Não foram encontrados arquivos locais de credenciais protegidas para descriptografia ou busca cruzada.
2. Identidade e diretórios: verificados caminhos absolutos, referências a projetos privados, identidade do autor, e-mails e IPs privados. Nenhum achado nos arquivos públicos. A autoria pública do manifesto usa o nome coletivo do projeto. Referências de compilação são configuráveis por `SdkDir`, `MOD_SDK_DIR` ou `sdk/`. Em execução, o diretório do mod vem de `modData.fullPath`, com fallback para a localização da própria assembly.
3. Artefatos e imagens: lista conferida com `git ls-files --others --exclude-standard`. DLLs, OGGs, builds, logs e análise local de áudio ficam fora dela. Não havia capturas de tela. O único visual público é um diagrama SVG criado para a documentação, com texto funcional sem dados privados. O pacote padrão foi conferido e não contém OGG.

Os resultados descrevem os arquivos preparados nesta data; não constituem garantia sobre alterações futuras.

## Triagem

| Mantido | Motivo |
| --- | --- |
| `src/` | Cópia dos fontes correspondentes ao mod |
| `tests/` | Verifica soltura, agendamento, cancelamento e limites visuais sem o jogo |
| Manifesto, projeto e build | Metadados e compilação independente |
| Documentação, diagrama e licença | Uso, limites técnicos e permissões |

| Ignorado / não incorporado | Motivo |
| --- | --- |
| DLL original | Compilado local, preservado no disco |
| Todos os OGG e análise de áudio | Redistribuição não autorizada; preservados apenas localmente |
| `bin/`, `obj/`, `dist/` | Gerados pelo build |
| Referências do jogo, stubs e backups | SDK externo e arquivos internos não necessários ao clone |
| Logs, scratch e planos de IA | Não são necessários à publicação |
| Credenciais e bancos locais | Não pertencem ao repositório |

Os projetos originais e backups externos a esta pasta foram preservados. O usuário optou explicitamente por excluir os áudios da publicação. Consulte `THIRD_PARTY.md` para alternativas locais.

## Validação

Build Release sem erros, com 3.617 verificações públicas aprovadas. Dois avisos MSB3277 vêm das referências reais de ThunderRoad: conflitos de versão de `System.Net.Http` e `System.IO.Compression`. Os avisos permanecem visíveis e exigem revisão com um conjunto de referências compatível com o destino antes de afirmar compatibilidade completa.

O build e os testes também passaram em uma cópia isolada contendo apenas os arquivos públicos, sem binários originais, OGG, stubs ou arquivos ignorados. As referências externas foram fornecidas explicitamente por parâmetro.

A validação de compilação usa referências PC disponíveis. Não houve execução Unity/IL2CPP, teste no Quest, medição de FPS ou sincronização perceptual. Sem OGG, o código aplica o impacto visual/físico sem o agendamento da faixa sonora. O pacote público não equivale à experiência sonora original.
