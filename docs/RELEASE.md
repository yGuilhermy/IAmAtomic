# I Am Atomic v1.2.0 — sem áudio

Inspirado no anime **The Eminence in Shadow**, este mod traz um ritual de Gravidade com circuitos, halos e impacto para Blade & Sorcery: Nomad.

## Destaques

- Ativação por tempo ou soltura do grip da mão livre.
- Circuitos procedurais, halos, ondas e partículas com três perfis de qualidade.
- Cancelamento ao pausar, perder foco, desativar o mod ou trocar de nível.
- Configuração pelo livro de opções do jogo.

## Áudios excluídos

**Este pacote não inclui arquivos OGG.** A autorização de redistribuição dos áudios não foi confirmada. Sem eles, o mod mantém os efeitos visuais e físicos e aplica o impacto sem aguardar a faixa sonora.

Áudios opcionais obtidos legalmente podem ser adicionados pelo usuário; consulte `THIRD_PARTY.md` dentro do pacote para nomes e requisitos de sincronização. A licença MIT do código não concede direitos sobre gravações de terceiros.

## Instalação

1. Extraia `IAmAtomic-v1.2.0-no-audio.zip`.
2. Copie a pasta `IAmAtomic` para a pasta `Mods` da instalação de Blade & Sorcery: Nomad.
3. Confira se `manifest.json` e `IAmAtomic.dll` estão diretamente dentro dessa pasta, sem uma pasta duplicada intermediária.
4. Substitua a versão anterior e reinicie o jogo. Para testar sem áudio, use uma instalação limpa do mod: substituir arquivos não remove OGG antigos.
5. Abra **Mod Options → I Am Atomic** e confira **Trigger Mode**. O jogo pode preservar a seleção anterior.

## Uso

No modo por tempo, segure uma arma imbuída com Gravidade até completar a carga. No modo de soltura, mantenha a arma imbuída em uma mão, selecione Gravidade na mão livre e segure o grip dessa mão; soltá-lo confirma o disparo após o debounce.

## Conteúdo

O pacote inclui a DLL do mod, manifesto, estas instruções, licença MIT e notas de dependências. Áudios, bibliotecas do jogo, símbolos de depuração e arquivos locais de desenvolvimento não estão incluídos.

## Validação e compatibilidade

Build Release concluído sem erros e 3.617 verificações de lógica aprovadas, incluindo uma validação em cópia isolada dos arquivos públicos.

Alvo informado pelo projeto: Nomad 1.3.1. A compilação utilizou referências PC disponíveis e apresentou dois avisos MSB3277 de conflito de versões de dependências do SDK. Não houve teste de execução Unity/IL2CPP, execução no Quest, medição de FPS ou ajuste perceptual de áudio. Recomenda-se marcar esta publicação como **pré-release** até concluir a validação no dispositivo.

## Licença

Código sob MIT; consulte `LICENSE`. As dependências mantêm as próprias licenças, descritas em `THIRD_PARTY.md` dentro do pacote.
