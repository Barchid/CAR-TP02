# ﻿Implémentation d’une passerelle REST vers un serveur FTP en ASP.NET Core 2
BARCHID Sami
25/02/2019

## Introduction
Le contenu de ce projet, développé en C# avec le framework ASP.NET Core 2, implémente une API REST permettant à un client REST de communiquer avec un serveur FTP distant.

Les commandes FTP gérées sont :
- PWD
- SYST
- AUTH
- USER
- PASS
- QUIT
- LIST
- TYPE
- PASV
- PORT
- PWD
- CDUP
- CWD
- RETR
- RNFR
- RNTO
- DELE
- RMD
- MKD
- STOR

**Attention :** les commandes liées à IPv6 ne sont pas supportées. Veillez donc à communiquer avec le serveur en IPv4.

## Guide d'utilisation
### Exécution
Lancer les commandes suivantes (à la racine du projet) :
```mvn package```
```java -jar target/CAR-TP01-1.0-SNAPSHOT.jar```

### Configurer le serveur FTP
Configurer le serveur FTP en manipulant le fichier `config.properties` situé à la racine du projet.

Champs de configurations possibles :
- `portNumber` : Le numéro de port du serveur
- `users` : Les utilisateurs disponibles dans le serveur
- `passwords` : Les mots de passe des utilisateurs
- `directories` : Les répertoires racine des utilisateurs


## Architecture


### Organisation des packages :
- **ftp** (package)
	- AppConfig.java
    - FtpCommand.java
    - FtpCommunication.java
    - FtpControlChannel.java
    -  FtpDataChannel.java
    -   FtpReply.java
    -   Main.java
    -   MockFtpDataChannel.java
    -   package-info.java
    -   SessionStore.java
  
 - **ftp.controls** (package)
	 -  FtpAuthControl.java
	 -  FtpCdupControl.java
	 -  FtpControl.java
	 -  FtpControlFactory.java
	 -  FtpCwdControl.java
	 -  FtpDataControl.java
	 -  FtpDeleControl.java
	  - FtpListDataControl.java
	 -  FtpMkdControl.java
	 -  FtpPassControl.java
	 - FtpPasvControl.java
	 -  FtpPortControl.java
	 -  FtpPwdControl.java
	 -  FtpQuitControl.java
	 -  FtpRetrDataControl.java
	 - FtpRmdControl.java
	 - FtpRnfrControl.java
	  - FtpRntoControl.java
	  - FtpStorDataControl.java
	   - FtpSystControl.java
	   - FtpTypeControl.java
	  - FtpUnknownControl.java
	  - FtpUserControl.java
	   - package-info.java

#### Package "ftp"
Le package **ftp** contient toutes les classes liées aux fonctionnalités internes du serveur FTP et qui permettent son bon fonctionnement qui n'est pas directement visible par l'utilisateur. Les fonctionnalités internes dont s'occupe ce package sont :
- La configuration générale du serveur
- Le démarrage du serveur
- La connection à un client 
- L'accès simultané pour plusieurs clients (multi-threading)
- La réception des commandes FTP et l'envoie des réponses au client en utilisant la couche transport
- La retenue d'informations sur le client au fil de son utilisation du service FTP

#### Package "ftp.controls"
Ce package contient des classes dont le but est de gérer chaque commande disponibles sur le serveur FTP. Ces classes sont des objets de contrôle qui implémenteront la logique pour chacune des commandes.

### Design patterns utilisés
- Pattern Strategy
	- Le pattern strategy a été utilisé pour la gestion des objets de contrôles 
- Control object
- Pattern Factory :
	- Le pattern Factory a été utilisé pour gérer 
	- La factory en question est la classe `ftp.controls.FtpControlFactory`
- Injection de dépendance + Singleton
	- La classe `AppConfig` est un singleton indirect qui est injecté par dépendance aux classes s'occupant de la gestion des utilisateurs pour que celles-ci puissent connaître les configurations des utilisateurs du serveur.
	- La classe `SessionStore` représente l'état d'une communication avec le client et les informations que le serveur retient pour ce client. Ce store est injecté par dépendance dans chaque classe qui a besoin des données.

### Gestion d'erreur
#### Gérer l'erreur du numéro de port déjà utilisé au démarrage du serveur
```java
try (ServerSocket server = new ServerSocket(appConfig.getPortNumber())) {
			System.out.println("Creating thread pool...");

			System.out.println("Waiting for clients...");
			while (true) {
				Socket client = server.accept();
				System.out.println("Client connection received from " + client.getInetAddress().toString());
				Runnable worker = new FtpCommunication(client, appConfig);
				new Thread(worker).start();
				System.out.println("Worker for client of ip (" + client.getInetAddress().getHostAddress() + ") ended.");
			}
		} catch (IOException e) {
			System.err.println("Cannot start FTP server : port number already used.");
		}
```

#### Gérer une erreur du réseau pendant la communication avec un client
Sert à éviter que le serveur crashe si le client coupe brusquement la connexion, etc.
```java
try (BufferedWriter controlOut = new BufferedWriter(
				new OutputStreamWriter(this.client.getOutputStream(), StandardCharsets.UTF_8));
				BufferedReader controlIn = new BufferedReader(
						new InputStreamReader(this.client.getInputStream(), StandardCharsets.UTF_8));) {

			FtpControlChannel controlChannel = new FtpControlChannel(controlOut, controlIn);
			this.initControls(controlChannel);

			System.out.println("Sending welcome message>");
			this.sendWelcomeMessage(controlChannel);

			[.......]

			// closing the client.
			this.client.close();
		} catch (IOException ex) {
			System.out.println(ex.getMessage());
			System.out.println("Error while receiving command/sending reply. Connection abort.");
		} catch (Exception ex) {
			System.out.println(ex.getMessage());
			System.out.println("Unknown error. Connection abort.");
		}
```
#### Gérer les erreurs de transmissions de données lors de l'échange de données avec le data channel
```java
try (Socket socket = new Socket(this.store.getActiveAdr().getAddress(), this.store.getActiveAdr().getPort())) {
			System.out.println("Writing IMAGE data through data channel with Image active mode.");
			this.writeImageData(data, socket);

			this.sendOpeningReply();
		} catch (IOException exception) {
			System.err.println("Could not open connection data. Send error to the control channel.");
			this.sendFailureReply();
		}
```

#### Gérer les erreurs du système de fichier (permission denied, file not found, etc) lors de la manipulation de l'arborescence de l'utilisateur
```java
try {
			Path path = Paths.get(parentPath, toDeletePath);
			if (!Files.exists(path)) {
				return new FtpReply(5, 5, 0, "File not found.");
			} else if (!Files.isDirectory(path)) {
				return new FtpReply(5, 5, 0, "Destination is not a directory.");
			} else {
				Files.delete(path);
				return new FtpReply(2, 5, 0, "File deleted successfully.");
			}
		} catch (SecurityException ex) {
			return new FtpReply(5, 5, 0, "Permission denied");
		} catch (InvalidPathException ex) {
			return new FtpReply(5, 5, 0, "Syntax error : path not valid.");
		} catch (DirectoryNotEmptyException ex) {
			return new FtpReply(5, 5, 0, "Directory not empty.");
		}
```

