# UnityFirebaseRestAPI
Basic libray of methods for Unity to acesss firebase using its REST API. Has methods to code and decode firebase data json.
Best part is **EVERYTHING** is done with Rest API. No firebase sdk needed. Only Librarys you need are Newtonsoft JSON .NET,
which is freely available and updated, and RestClient, which is freely available on the Unity Asset Store.

***Tested with Unity 2022.3.21f***

## Methods included

**SignIn()**

*NewSignIn(string email, string password, SignInCallback callback, bool signup = false)*

Used to sign in to firebase

**GetDoc()**

*NewGetDoc<T>(string path, string doc, GetDocCallback callback)*
 
Gets a single document from firebase, returns as json data to be converted to Data Structure of your choosing

**GetDocs()**

*GetDocs(string path, GetDocIDsCallback callback)*

Gets list of documents from firebase, returns a String List of documents in specified path

**WriteDoc()**

*NewWriteDoc<T>(object data, string path, string docId, PostDocCallback callback)*

Creates or Updates specified document using past data.

