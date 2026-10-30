<h1 align="center">I Am Atomic</h1>

<p align="center">Um ritual de Gravidade inspirado em <strong>The Eminence in Shadow</strong>, dentro de Blade &amp; Sorcery: Nomad.</p>

<p align="center">
  <a href="https://github.com/yGuilhermy/IAmAtomic/releases/tag/v1.2.0"><strong>Baixar v1.2.0</strong></a> ·
  <a href="#instalar">Instalação</a> ·
  <a href="#usar">Como usar</a> ·
  <a href="#perguntas-frequentes">FAQ</a> ·
  <a href="https://github.com/yGuilhermy/IAmAtomic/issues">Reportar problema</a>
</p>

<p align="center">C# / .NET Standard 2.1 · Código aberto sob MIT · Pré-release sem áudio</p>

---

Carregue uma arma imbuída com Gravidade e libere um impacto cercado por circuitos, halos e ondas procedurais. Escolha entre um ritual com duração definida ou um disparo controlado pela soltura da mão livre; ajuste os efeitos no próprio livro do jogo.

> [!IMPORTANT]
> Esta versão é uma **pré-release**, com alvo em Nomad 1.3.1 e validação no Quest ainda pendente. O download **não inclui áudios**; os efeitos visuais e físicos funcionam sem as gravações.

## Instalar

Para instalar o pacote pronto, você precisa de Blade & Sorcery: Nomad com suporte a mods de script compatíveis. Não é necessário ter .NET ou compilar o projeto no computador.

### 1. Baixe o pacote

| Arquivo | Finalidade |
| --- | --- |
| [IAmAtomic-v1.2.0-no-audio.zip](https://github.com/yGuilhermy/IAmAtomic/releases/download/v1.2.0/IAmAtomic-v1.2.0-no-audio.zip) | Mod pronto para instalação, sem OGG |
| [IAmAtomic-v1.2.0-no-audio.zip.sha256](https://github.com/yGuilhermy/IAmAtomic/releases/download/v1.2.0/IAmAtomic-v1.2.0-no-audio.zip.sha256) | Hash SHA-256 para conferir o download |

### 2. Copie para o jogo

1. Feche o jogo e extraia o ZIP.
2. Copie a pasta `IAmAtomic` para a pasta `Mods` da instalação de Nomad.
3. Confira a estrutura abaixo e reinicie o jogo.

```text
Mods/
└── IAmAtomic/
    ├── IAmAtomic.dll
    ├── manifest.json
    ├── LICENSE
    ├── README.md
    └── THIRD_PARTY.md
```

Ao atualizar, substitua a instalação anterior e evite duas cópias do mod. Para testar o pacote sem áudio, use uma pasta limpa: copiar arquivos por cima não remove OGG antigos.

## Usar

Abra **Mod Options → I Am Atomic** no livro e confira **Trigger Mode**. A configuração escolhida fica fixada durante cada ritual.

| Modo | Como ativar | Como disparar |
| --- | --- | --- |
| Por tempo (`Direct`) | Segure uma arma imbuída com Gravidade | Complete o tempo de carga selecionado |
| Por soltura (`HoldRelease`) | Segure a arma imbuída; na mão livre, selecione Gravidade e mantenha o grip fechado | Solte o grip da mão livre; há debounce de 20 ms |

**Channeling Time** define a carga no modo por tempo e a expansão visual no modo de soltura. Configurações salvas pelo jogo podem manter um modo antigo após a atualização.

## Recursos

| Recurso | O que oferece |
| --- | --- |
| Dois modos de ativação | Carga com duração definida ou liberação pela mão livre |
| Efeitos procedurais | Circuitos, halos, ondas e partículas |
| Três perfis visuais | 32 / 64 / 96 circuitos; limites de 96 / 192 / 288 partículas |
| Controle de partículas | Opção para desligá-las pelo menu |
| Áudio opcional | Agendamento pelo relógio DSP e ajuste fino com arquivos compatíveis |
| Cancelamento do ritual | Limpeza ao pausar, perder foco, desativar o mod ou trocar de nível |

### Áudio opcional

Os OGG foram excluídos porque a autorização de redistribuição não foi confirmada. O código mantém o impacto visual e físico quando não encontra os arquivos sonoros.

Para uso local, você pode adicionar arquivos que tenha direito de utilizar. Consulte [dependências e áudio](docs/THIRD_PARTY.md) para os nomes e o marcador de sincronização esperado. Com uma faixa compatível, o modo por tempo acrescenta a introdução sonora e a compensação do buffer após a carga; o modo de soltura usa o trecho de impacto direto.

## Perguntas frequentes

### Por que não ouço o ataque?

O release público é distribuído sem áudio. Isso é esperado; veja [áudio opcional](#áudio-opcional).

### A atualização mudou o modo de ativação?

O jogo pode preservar a seleção anterior. Confira **Trigger Mode** no livro antes de iniciar um ritual.

### Qual perfil visual devo escolher?

Comece pelo perfil Equilibrado, que é o padrão, e ajuste conforme sua experiência. O Cinemático permite mais circuitos e partículas; ainda não há medições de FPS no Quest para recomendar um perfil por dispositivo.

### Esta versão já foi testada no headset?

Ainda não. O build Release e 3.617 verificações de lógica passaram, inclusive em uma cópia isolada dos arquivos públicos. Isso não substitui testes de execução Unity/IL2CPP, desempenho e sincronização no Quest. Veja o [relatório de validação](docs/AUDIT.md).

## Desenvolvimento

Os testes de lógica podem ser executados sem instalar o jogo. Para compilar a DLL completa, são necessárias referências externas compatíveis de ThunderRoad e Unity.

<details>
<summary><strong>Pré-requisitos, compilação e testes</strong></summary>

### Pré-requisitos

- Windows com PowerShell.
- SDK .NET 8 ou superior.
- Referências obtidas de uma instalação ou SDK autorizado: `ThunderRoad.dll`, `Newtonsoft.Json.dll`, `UnityEngine.dll` e os módulos `UnityEngine.*.dll` correspondentes.

As bibliotecas do jogo não são distribuídas neste repositório. Stubs não substituem essas referências em um build jogável.

### Compilar

```powershell
git clone https://github.com/yGuilhermy/IAmAtomic.git
cd IAmAtomic
./build.ps1 -SdkDir (Read-Host 'Pasta das DLLs de referencia')
```

O script compila em Release, executa os testes e cria `dist/IAmAtomic/`. Também aceita `MOD_SDK_DIR`; sem parâmetro ou variável, procura as referências em `sdk/`, relativa à raiz do projeto.

Para empacotar áudios autorizados apenas para uso local, adicione os arquivos em `audio/` e informe `-IncludeLocalAudio`. Essa opção não concede direitos de redistribuição.

### Testar a lógica

```powershell
dotnet run --project tests/LogicTests.csproj -c Release
```

A suíte cobre soltura, debounce, cancelamento, impacto único, limites de sincronização e orçamento visual.

### Limites do build verificado

As referências PC utilizadas geram dois avisos MSB3277, relacionados a `System.Net.Http` e `System.IO.Compression`. O build conclui sem erros, mas a compatibilidade binária com Nomad precisa de validação no destino. O manifesto mantém `GameVersion` em `1.0.0.0`; esse campo não comprova compatibilidade com outras versões.

</details>

## Documentação

| Documento | Conteúdo |
| --- | --- |
| [Notas do release](docs/RELEASE.md) | Pacote, instalação e limites da versão |
| [Dependências e áudio](docs/THIRD_PARTY.md) | Referências externas e arquivos sonoros opcionais |
| [Auditoria e validação](docs/AUDIT.md) | Preparação para publicação e verificações realizadas |

O código não implementa telemetria, autenticação ou envio a serviços externos. As opções usam o sistema de mods do jogo; o carregamento de áudio lê arquivos locais.

## Contribuir

Para reportar um problema, [abra uma issue](https://github.com/yGuilhermy/IAmAtomic/issues) com a versão do jogo, versão do mod, modo de ativação, perfil visual e passos para reproduzir. Se anexar logs, remova informações pessoais.

Pull requests devem descrever a alteração e como ela foi validada. Para mudanças de lógica, execute a suíte; para efeitos e comportamento em VR, informe se houve teste no headset.

## Licença

Código sob [MIT](LICENSE). A inspiração em **The Eminence in Shadow** não inclui a distribuição de gravações do anime. Jogo, SDK e outros materiais de terceiros mantêm suas próprias licenças.
