
# ﻿Implémentation d’une passerelle REST vers un serveur FTP en ASP.NET Core 2
BARCHID Sami
25/02/2019

## Introduction
Le contenu de ce projet, développé en C# avec le framework ASP.NET Core 2, implémente une API REST permettant à un client REST de communiquer avec un serveur FTP distant.

La passerelle REST dévelopée permet de réaliser les opérations suivantes :
- Se connecter à un utilisateur FTP.
- Lister un dossier.
- Créer un dossier.
- Supprimer un dossier.
- Renommer un dossier.
- Uploader un dossier (sous forme d'un zip).
- Télécharger un dossier (sous forme d'un zip).
- Télécharger un fichier.
- Renommer un fichier.
- Supprimer un fichier.
- Uploader un fichier.

## Guide d'utilisation
### Installation
Le SDK .NET Core est la version Open Source et cross-platform de l'ancien framework .NET de Microsoft. 
Ce SDK est requis pour pouvoir compiler et lancer le projet. Pour l'installer, il suffit de suivre le guide officiel d'installation afin d'obtenir `dotnet`, l'interface en ligne de commande. Lien à suivre : https://dotnet.microsoft.com/download?initial-os=linux.

### Lancement des tests
À la racine du projet, lancer la commande `dotnet test` afin de visualiser les tests réalisés.

### Exécution
Pour construire un build de déploiement du programme, utiliser la commande `dotnet publish` à la racine du projet.

Ensuite, lancer le build créé en production avec la commande suivante :
`dotnet ./WebApi/bin/Debug/netcoreapp2.0/WebApi.dll`.

### Configurer l'API REST de passerelle FTP
L'API REST peut être configurée en remplissant le fichier `appsettings.json`.

Il y a deux champs de configurations :
- `"Host"` : L'adresse IP du serveur FTP dont l'API REST sera la passerelle.
- `"Port"` : le numéro de port du serveur FTP pour communiquer.


## Architecture
L'architecture de l'API REST a suivi les recommandations de base du modèle MVC enseignée dans la documentation officielle du framework ASP.NET Core 2.

### Organisation des projets:
Il y a deux grands projets dans l'application : 
- **WebApi** : Un projet "ASP.NET Core Web API" qui gère l'implémentation de la passerelle REST.
- **Tests** : un projet de tests Xunit pour rassembler les tests unitaires de l'application.


### Architecrue du projet "WebApi"
- **WebApi.Controllers** (namespace)
	- FilesController.cs
	- DirectoriesController.cs
  
 - **WebApi.Ftp** (namespace) : *contient les services permettant l'interaction avec le serveur FTP distant.* 
	 - IClient.cs (interface)
	 - Client.cs
 - **WebApi.Model** (namespace) : *contient les classes de Model qui font le lien entre le client et l'API REST.*
	 - MoveInput.cs
 - **WebApi.Tools** (namespace) : *Outils additionnels propres aux spécificités du framework ASP.NET Core pour simplifier certaines tâches redondantes/améliorer le rendu du serveur FTP.*
	 - ErrorHandlingMiddleware.cs
	 - FtpContext.cs
	 - FtpCredentialExtension.cs
	 - PassHeaderFilter.cs
	 - UserHeaderFilter.cs
 - **WebApi** (namespace) : *namespace de démarrage du serveur, c'est ici que l'élaboration des injections de dépendances, du chargement de la configuration et des middlewares est réalisée*
	 - Startup.cs
	 - Program.cs - *(classe Main par défaut dans un projet ASP.NET Core 2)* 


## Code samples
### Pattern : injection de dépendance
Installation et mise à disposition automatique des services généraux de l'application pour les autres classes afin d'éviter la complexité de gérer les dépendances à la main. Les dépendances sont alors injectées grâce à un simple référencement dans le constructeur.

```csharp
// Startup.cs
[...]
public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<FtpContext>();
            // référencer le service IClient qui sera injecté via dépendance
            services.AddScoped<IClient, Client>();
	        [...]
```

```csharp
// DirectoriesController.cs, classe utilisant le service IClient.
	[...]
    public class DirectoriesController : Controller
    {
        private readonly IClient _client;

        public DirectoriesController(IClient client)
        {
            _client = client;
        }
        [...]
```
	 
### Modèle MVC
Implémentation d'un modèle MVC classique pour la communication avec le client REST et le serveur FTP tout en respectant le principe de "Separation of concern".

### Middleware : sérialisation d'exception en erreur HTTP
Morceau de code placé après l'exécution des controllers qui permet de récupérer une exception imprévue par l'application pour la formatter en une réponse HTTP valide.

```csharp
// ErrorHandlingMiddleware.cs
[...]
private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
	        // Définition du code d'erreur de l'exception attrapée.
            HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected

            bool isBadRequest = exception is FtpCommandException || exception is ArgumentOutOfRangeException ||
                exception is FtpException || exception is InvalidDataException;

            if (isBadRequest)
            {
                code = HttpStatusCode.BadRequest;
            }

			// renvoie du message d'erreur de l'exception sous JSON
            string result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            // envoi de la réponse d'erreur
            return context.Response.WriteAsync(result);
        }
        [...]
```


### État de l'application pour une requête unique
Pour maintenir la norme stateless d'une architecture REST, une classe est créée  où est garantie l'instance unique au sein de la même requête. Cette classe contient des données telles que le mot de passe utilisé pour logger l'utilisateur FTP.

```csharp
// FtpContext.cs
[...]
public class FtpContext
    {
        public string User { get; set; }
        public string Pass { get; set; }
    }
    [...]
```
```csharp
// Startup.cs
[...]
services.AddScoped<FtpContext>(); // "AddScoped" force l'instance unique de FtpContext pour chaque requête à l'API.
[...]
```

### Middleware : connexion d'un utilisateur à un serveur FTP
Pour garantir le respect de la norme Stateless d'une API REST, un utilisateur doit fournir ses identifiants à chaque requête. Un middleware placé avant l'exécution des controllers permet de récupérer les identifiants d'un utilisateur placé en en-tête de la requête HTTP.

```csharp
[...]
public async Task InvokeAsync(HttpContext context)
        {
            FtpContext ftpContext = context.RequestServices.GetService<FtpContext>();

			// récupération du nom d'utilisateur
            string userInput = context.Request.Headers["USER"].ToString() ?? "anonymous";
			// récupération du mot de passe
            string passInput = context.Request.Headers["PASS"].ToString() ?? "anonymous";

			// connexion
            ftpContext.User = userInput;
            ftpContext.Pass = passInput;
			
			// passage à l'exécution des controllers
            await _next(context);
        }
        [...]
```

## Gestion d'erreurs
La spécificité de l'architecture et du framework ASP.NET Core 2 permet à l'application de tourner sans avoir besoin de prévoir de cas spécifiques pour la gestion d'erreur, ce qui facilite grandement l'élaboration du code.

## Exemples de requêtes avec CURL
- Lister le répertoire "/Conception" : 
	- ***curl -X GET "http://localhost:57876/api/Directories/list?path=%2FConception" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol"***

- Télécharger le répertoire "/Conception" sous un zip nommé "Conception.zip" :
	- ***curl -X GET "http://localhost:57876/api/Directories/download?path=%2FConception" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol" -o "Conception.zip"***

- Renommer un dossier "/Conception" en un dossier "/Conceptions" :
	- ***curl -X PUT "http://localhost:57876/api/Directories" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol" -H "Content-Type: application/json-patch+json" -d "{ \"oldPath\": \"/Conception\", \"targetPath\": \"/Conceptions\"}"***

- Créer un dossier vide "/oui" :
	- ***curl -X POST "http://localhost:57876/api/Directories?path=%2Foui" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol"***

- Supprimer un dossier "/oui" :
	- ***curl -X DELETE "http://localhost:57876/api/Directories?path=%2Foui" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol"***

- Upload le dossier sous format zip "Conception.zip" à l'emplacement "/Conconception" :
	- ***curl -X POST "http://localhost:57876/api/Directories/Upload?path=%2FConconception" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol" -H "Content-Type: multipart/form-data" -F "archive=@lol.zip;type=application/x-zip-compressed"***

- Télécharger un fichier "/license.txt" : 
	- ***curl -X GET "http://localhost:57876/api/Files/download?path=%2Flicense.txt" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol" -o "license.txt"***

- Upload un fichier "/lilicense.txt" :
	- ***curl -X POST "http://localhost:57876/api/Files?path=%2Flilicense.txt" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol" -H "Content-Type: multipart/form-data" -F "file=@license.txt;type=text/plain"***

- Renommer un fichier "/lilicense.txt" en un fichier "/lissansse.TéIxTé" :
	- ***curl -X PUT "http://localhost:57876/api/Files" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol" -H "Content-Type: application/json-patch+json" -d "{ \"oldPath\": \"/lilicense.txt\", \"targetPath\": \"/lissansse.TéIxTé\"}"***

- Supprimer le fichier "/lissansse.TéIxTé" :
	- ***curl -X DELETE "http://localhost:57876/api/Files?path=%2Flissansse.T%C3%A9IxT%C3%A9" -H "accept: application/json" -H "USER: barchid" -H "PASS: lol"***


Une documentation de l'API en ligne est disponible lors du lancement du projet à la route suivante :
**http://localhost:57876/swagger/index.html** (cette page simule des requêtes avec CURL directement).

## Notes importantes :
L'API REST implémentée a été pensée pour être 100% stateless, et par conséquent, le client REST doit fournir les identifiants de l'utilisateur FTP à chaque requête car le serveur ne conserve pas la connexion FTP entre deux requêtes.

Une critique pouvant être faite est le manque d'optimalité de la solution puisque la passerelle REST doit se reconnecter à chaque requête. Cependant, comme mentionné dans les nombreux liens ci-dessous, une architecture REST se doit d'être stateless. C'est pourquoi le fait de garder une connexion entre deux requêtes reviendrait à ne pas implémenter une API REST. De ce fait, l'implémentation moins optimale a été gardée dans ce projet.

 - Références :
	 - https://fr.wikipedia.org/wiki/Representational_state_transfer
	 - https://stackoverflow.com/questions/3105296/if-rest-applications-are-supposed-to-be-stateless-how-do-you-manage-sessions
	 - https://restfulapi.net/statelessness/
