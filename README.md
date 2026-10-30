# I Am Atomic

Um ritual de Gravidade com circuitos, halos e impacto para Blade & Sorcery: Nomad.

Inspirado no anime **The Eminence in Shadow**, este mod recria a ideia do ataque “I Am Atomic” com efeitos procedurais dentro do jogo.

**Versão:** 1.2.0 · **Tecnologia:** C# / .NET Standard 2.1 · **Licença:** [MIT](LICENSE)

## Funcionalidades

- Modo por tempo (`Direct`): segure uma arma imbuída com Gravidade até completar a carga.
- Modo de soltura (`HoldRelease`): segure a arma imbuída, selecione Gravidade na mão livre e mantenha o grip dessa mão fechado. Soltar o grip confirma o disparo após debounce de 20 ms.
- Circuitos procedurais, halos, ondas e partículas, com três perfis de qualidade.
- Agendamento de impacto com relógio DSP quando há áudio disponível.
- Cancelamento e limpeza ao pausar, perder foco, desativar o mod ou trocar de nível.

## Áudio opcional

**Este repositório e o pacote público padrão não incluem OGG.** A autorização de redistribuição das gravações locais não foi confirmada. Sem os arquivos, o mod mantém efeitos visuais e físicos e aplica o impacto sem aguardar a faixa sonora.

Consulte [os arquivos opcionais e o marcador de sincronização](docs/THIRD_PARTY.md). Somente para uso local com áudios que você tem direito de utilizar, o script aceita `-IncludeLocalAudio`. Essa opção não concede autorização de redistribuição.

## Uso no jogo

Abra **Mod Options → I Am Atomic** no livro e confira **Trigger Mode**, inclusive depois de atualizar: o jogo pode preservar configurações antigas. Imbua a arma com Gravidade e siga o modo escolhido.

| Opção | Uso |
| --- | --- |
| Trigger Mode | Por tempo ou soltura da mão livre |
| Channeling Time | Tempo de carga no modo por tempo; expansão visual no modo de soltura |
| Qualidade visual | 32/64/96 circuitos e limites de 96/192/288 partículas |
| Ajuste fino de sincronização | Corrige a percepção de áudio quando arquivos compatíveis estão presentes |

Com áudio compatível, o modo por tempo acrescenta o trecho inicial da faixa e a compensação do buffer depois da carga. O modo de soltura usa o arquivo de impacto direto. A latência física do headset não foi medida.

## Compilar a partir do código

Pré-requisitos: Windows com PowerShell, SDK .NET 8 ou superior e DLLs de referência compatíveis de ThunderRoad/Unity, obtidas de uma instalação ou SDK autorizado. O jogo não está incluído. Não use stubs para produzir um mod jogável.

1. Clone somente este repositório.
2. Coloque as referências em uma pasta externa ou em `sdk/` (ignorada pelo Git). Ela deve conter `ThunderRoad.dll`, `Newtonsoft.Json.dll`, `UnityEngine.dll` e os módulos `UnityEngine.*.dll` correspondentes.
3. Na raiz do clone, execute:

```powershell
./build.ps1 -SdkDir (Read-Host 'Pasta das DLLs de referencia')
```

Também é possível definir `MOD_SDK_DIR` ou usar `./build.ps1` com as referências em `sdk/`. O caminho é resolvido em tempo de execução e não depende da máquina do autor.

Para executar os testes de lógica sem jogo ou referências proprietárias:

```powershell
dotnet run --project tests/LogicTests.csproj -c Release
```

O build cria `bin/`, `obj/` e `dist/` automaticamente. A saída instalável fica em `dist/IAmAtomic/`. Copie somente essa pasta para a pasta `Mods` da instalação do jogo, usando o gerenciador de arquivos do dispositivo. Fontes, testes e bibliotecas de referência não precisam ser instalados no headset. Reinicie o jogo e evite duas cópias do mesmo mod.

## Comandos de desenvolvimento

| Comando / parâmetro | Resultado |
| --- | --- |
| `./build.ps1 -SdkDir <pasta>` | Compila Release, executa testes e gera o pacote |
| `MOD_SDK_DIR` | Define a pasta de referências sem editar o projeto |
| `dotnet run --project tests/LogicTests.csproj -c Release` | Executa os testes públicos de lógica |

## Privacidade

O código não implementa telemetria, autenticação ou envio de dados a serviços externos. As opções são expostas pelo sistema de mods do jogo. Binários, símbolos de depuração, referências proprietárias, logs, credenciais e arquivos locais estão excluídos do Git. Não há caminhos pessoais nos arquivos preparados para publicação.

## Estrutura do repositório

| Pasta / arquivo | Finalidade |
| --- | --- |
| `src/` | Código C# do mod |
| `tests/` | Testes públicos de lógica sem dependência do jogo |
| `manifest.json` | Metadados lidos pelo jogo |
| `IAmAtomic.csproj` e `build.ps1` | Compilação e empacotamento independentes |
| `docs/AUDIT.md` | Auditoria e limites da validação |
| `docs/THIRD_PARTY.md` | Dependências e direitos de terceiros |

## Validação e limites

Build Release e testes públicos de mão fechada prolongada, debounce de soltura, cancelamento, impacto único, limites de sincronização e orçamento visual.

A compilação local usa referências reais da instalação PC disponível. Ela apresenta dois avisos MSB3277 de conflito entre versões de `System.Net.Http` e `System.IO.Compression` exigidas pelo SDK. Os avisos não foram ocultados. Não houve execução no Unity/IL2CPP, teste visual, medição de FPS ou teste no Quest nesta preparação. A compatibilidade com Nomad 1.3.1 é o alvo informado pelo projeto, ainda dependente de validação no dispositivo. O campo `GameVersion` do manifesto foi preservado em `1.0.0.0`; ele não representa uma confirmação de compatibilidade com todas as versões do jogo.

## Licença

Código sob [MIT](LICENSE), escolhida pela permissão simples de uso, modificação e redistribuição. Jogo, SDK e outros materiais de terceiros não são relicenciados; consulte [as notas de dependências](docs/THIRD_PARTY.md).
