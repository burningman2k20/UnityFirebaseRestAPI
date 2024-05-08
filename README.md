# UnityFirebaseRestAPI
Basic libray of methods for Unity to acesss firebase using its REST API. Has methods to code and decode firebase data json.
Best part is **EVERYTHING** is done with Rest API. No firebase sdk needed.

## Methods included

**SignIn()**

Used to sign in to firebase

**GetDoc()**

* *NewGetDoc<T>(string path, string doc, GetDocCallback callback)* *
* 
Gets a single document from firebase, returns as json data to be converted to Data Structure of your choosing

**GetDocs()**

* *GetDocs(string path, GetDocIDsCallback callback)* *

Gets list of documents from firebase, returns a String List of documents in specified path

**WriteDoc()*

* *NewWriteDoc<T>(object data, string path, string docId, PostDocCallback callback)* *

Creates or Updates specified document using past data.

