# Audio Celular PC (USB)

## Sobre
Este utilitário permite reproduzir o áudio do seu celular Android diretamente no PC quando conectado via USB, sem precisar configurar manualmente o scrcpy. 

## Características
- **Três modos disponíveis**:
  - Apenas áudio (sem janela)
  - Áudio + espelhamento de tela
  - Apenas espelhamento (sem áudio)
- **Download automático** do scrcpy na primeira execução
- **Detecção automática** do dispositivo Android via ADB
- **Executável único** - não requer instalação

## Pré-requisitos
- Windows 10/11 (64-bit)
- Celular Android 11+ (funciona melhor com Android 12+)
- Depuração USB habilitada no celular
- Cabo USB de dados

## Como usar

### 1. Preparar o celular
1. Vá em **Configurações** > **Sobre o telefone**
2. Toque 7 vezes em **Número da compilação** para ativar modo desenvolvedor
3. Vá em **Configurações** > **Opções do desenvolvedor**
4. Ative **Depuração USB**

### 2. Conectar via USB
1. Conecte o celular ao PC com cabo USB
2. No celular, selecione **Transferência de arquivos (MTP)**
3. Quando aparecer o popup "Permitir depuração USB", marque "Sempre permitir" e toque em **OK**

### 3. Executar
1. Execute `AudioCelularPC.exe`
2. Na primeira execução, aguarde o download do scrcpy (~50MB)
3. Escolha o modo desejado no menu

## Arquitetura e Funcionamento

### Fluxo Principal
O <mcfile name="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs"></mcfile> implementa uma arquitetura modular:

1. **<mcsymbol name="Main" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="14" type="function"></mcsymbol>**: Ponto de entrada que orquestra todo o fluxo
2. **<mcsymbol name="EnsureScrcpyAsync" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="65" type="function"></mcsymbol>**: Gerencia download e extração do scrcpy
3. **<mcsymbol name="EnsureAdbDeviceAsync" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="78" type="function"></mcsymbol>**: Verifica conectividade do dispositivo
4. **<mcsymbol name="LaunchScrcpyInteractive" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="115" type="function"></mcsymbol>**: Executa o scrcpy com redirecionamento de I/O

### Métodos de Modo
- **<mcsymbol name="StartAudioOnlyAsync" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="95" type="function"></mcsymbol>**: Usa `--no-video --no-control` para modo apenas áudio
- **<mcsymbol name="StartAudioAndMirrorAsync" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="102" type="function"></mcsymbol>**: Configuração padrão com áudio e vídeo
- **<mcsymbol name="StartMirrorNoAudioAsync" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="108" type="function"></mcsymbol>**: Usa `--no-audio` para apenas espelhamento

### Utilitários Auxiliares
- **<mcsymbol name="DownloadFileAsync" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="131" type="function"></mcsymbol>**: Download via HttpClient
- **<mcsymbol name="RunProcessAsync" filename="Program.cs" path="C:\Users\pedro\dev\audio_celular_pc\Program.cs" startline="139" type="function"></mcsymbol>**: Execução de processos com captura de saída

## Solução de Problemas

### Dispositivo não detectado
- Verifique se a depuração USB está ativa
- Teste diferentes cabos USB (alguns são apenas para carregamento)
- Reinicie o processo adb com opção "R" no menu

### Audio não funciona
- Certifique-se que o Android é versão 11+
- No Android 11, mantenha a tela desbloqueada durante a conexão
- Verifique se não há outros aplicativos usando o áudio

### Erro de download
- Verifique a conexão com a internet
- Execute como administrador se necessário
- Desative temporariamente antivírus durante o download

## Compilação

Para compilar o projeto:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true
```

O executável será gerado em `bin\Release\net8.0\win-x64\publish\AudioCelularPC.exe`

## Tecnologias Utilizadas
- **.NET 8.0** - Framework principal
- **scrcpy v3.3.1** - Baixado automaticamente do GitHub
- **System.IO.Compression** - Para extração de ZIP
- **System.Diagnostics.Process** - Para execução do scrcpy/adb

## Licença
Este projeto utiliza o scrcpy (Apache 2.0 License) e é distribuído sob os mesmos termos.
