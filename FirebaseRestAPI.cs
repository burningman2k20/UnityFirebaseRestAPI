using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;

using Proyecto26;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;

[Serializable]
public class RestFirebaseUser {
// localId	string	The uid of the current user.
    public string localId;
    public string UserId;
// email	string	The email of the account.
    public string email;
// emailVerified	boolean	Whether or not the account's email has been verified.
    public bool emailVerified;
// displayName	string	The display name for the account.
    public string displayName;
// providerUserInfo	List of JSON objects	List of all linked provider objects which contain "providerId" and "federatedId".
    // public Dictionary<string, string> providerUserInfo;
// photoUrl	string	The photo Url for the account.
    public string photoUrl;
// passwordHash	string	Hash version of password.
    public string passwordHash;
// passwordUpdatedAt	double	The timestamp, in milliseconds, that the account password was last changed.
    public double passwordUpdatedAt;
// validSince	string	The timestamp, in seconds, which marks a boundary, before which Firebase ID token are considered revoked.
    public string validSince;
// disabled	boolean	Whether the account is disabled or not.
    public bool disabled;
// lastLoginAt	string	The timestamp, in milliseconds, that the account last logged in at.
    public string lastLoginAt;
// createdAt	string	The timestamp, in milliseconds, that the account was created at.
    public string createdAt;
// customAuth	boolean	Whether the account is authenticated by the developer.
    public bool customAuth;
    public string refreshToken;
}
public class FirebaseRestAPI//  : MonoBehaviour 
{

    public static RestFirebaseUser currentUser;

    private static fsSerializer serializer = new fsSerializer();


/* Call backs for REST API functions */
    public delegate void SignInCallback(RestFirebaseUser user);
    public delegate void PostDocCallback(string user);
    public delegate void GetDocCallback(JObject user);// where T : new();
     public delegate void GetDocIDsCallback(List<string> documents);

    private static string authToken = "";
    private string userID = "";
    private const string authBaseUrl = "https://identitytoolkit.googleapis.com/v1/accounts";
     private const string projectId = "YOUR_PROJECT_ID"; // You can find this in your Firebase project settings
    private const string baseUrl = "https://firestore.googleapis.com/v1/projects/" + projectId +"/databases/(default)/documents";
    private const string dbUrl = "projects/" + projectId + "/databases/(default)/documents";
    private const string apiKey = "YOUR_API_KEY;

    public string email;
    public string password;

    private string collectionPath = "YOUR_COLLECTION";
    private string documentId = "YOUR_DOCUMENT_ID";

    public static JObject json = new JObject();
    
    // using ValueObject = System.Collections.Generic.Dictionary<string, object>;

     public struct FirestoreDocument
    {
        public string name;
        public DateTime createTime;
        public DateTime updateTime;
        //The real content is here, but they are still in "___Value" x object format. Dict key is the field's name. Inner dict key is the ___Value text.
        public Dictionary<string, object> fields;
    }

      // Method to create a stringValue JSON object
    private JObject CreateStringValue(string key, string value) {
        JObject stringValueJson = new JObject();
        JObject json = new JObject();
        stringValueJson["stringValue"] = value;
        json[key] = stringValueJson;
        return json;
    }
    private JObject ConvertDictionaryValueToJObject(object value) {
        JObject jsonObject = new JObject();

        // Check if the value is a dictionary
        if (value is Dictionary<string, object>) {
            Dictionary<string, object> dictionaryValue = (Dictionary<string, object>)value;

            // Convert nested dictionary value to JObject recursively
            foreach (var pair in dictionaryValue) {
                jsonObject[pair.Key] = JToken.FromObject(pair.Value);
            }
        }

        return jsonObject;
    }


/* start of code borrowed from other project */

/// <summary>
    /// Gets all users from the Firebase Database
    /// </summary>
    /// <param name="callback"> What to do after all users are downloaded successfully </param>
    public void GetDocs(string path, GetDocIDsCallback callback)
    {
        JObject json = new();
        
        RequestHelper req = new RequestHelper {
            Uri = $"{baseUrl}/{path}/",
            Method = "GET",
            
            IgnoreHttpException = true, //Prevent to catch http exceptions
            ParseResponseBody = true, //Don't encode and parse downloaded data as JSON
            ContentType = "application/json",
            EnableDebug = false,
            DefaultContentType = false,
            Headers = new Dictionary<string, string> { 
                { "Authorization", $"Bearer {authToken}" },
                
                },
            Params = new Dictionary<string, string>
                {
                    {"key", (string)apiKey},//  FirebaseConfig.api}
                },
            
           
            
    };
        
        RestClient.Get(req).Then(response => {
            // Waiting(500);
            
            JObject obj = JObject.Parse(response.Text);
            JObject json = new();
            var docIds = ParseCollectionIds(obj);
          
            callback(docIds);
        }).Catch(err => {
            EditorUtility.DisplayDialog ("Error", err.Message, "Ok");
        });
    }

/// <summary>
/// Parses and Returns List of Collections from firebase json data
/// </summary>
/// <param name="json"></param>
/// <returns>
/// List<string>
/// </returns> 

private static List<string> ParseCollectionIds(JObject json){
    List<string> returnList = new List<string>();
    var list = (JArray)json.SelectToken("documents");
    foreach(var item in list){
        var str = (string)item.SelectToken("name");
        var _list = str.Split("/");
        returnList.Add(_list.Last());
        // Debug.Log($"{_list.Last()}");
            // item.SelectToken("name"));
    }
return returnList;
}


/* Method to create update field(s) for firebase rest api*/
private static List<string> getUpdateFields(object obj){
        
        
        Type objType = obj.GetType();

        var objField = objType.GetFields();//.GetValue(0);//GetValue(1);
        List<string> fields = new();

        foreach (var member in objField){
            fields.Add(member.Name);
        }
        

        return fields;
    }

/* Method to create fields for updating firebase using json*/
     private static JObject createField(string name, string type, object value){
        
        var obj = new JObject();
        JToken _value;

        // obj = new JObject();

        string _type = "";
        if (type == "System.String") {
            _type = "stringValue";
            _value = (string)value;
        } else if(type == "System.Boolean"){
            _type = "booleanValue";
            _value = (bool)value;
        } else if (type == "System.Double"){
            _type = "doubleValue";
            _value = (double)value;
        } 
        else if (type == "System.Single"){
            _type = "doubleValue";
            _value = (double)value;
        }
        else if (type == "System.Int16"){
            _type = "integerValue";
            _value = (int)value;
        }
        else if (type == "System.Int32"){
            _type = "integerValue";
            _value = (int)value;
        }
        else if (type == "System.Int64"){
            _type = "integerValue";
            _value = (int)value;
        }
        else {
            _type = "unknownValue";
            _value = (string)value;
        }
        if (value == null) {
            _type = "nullValue";
            _value = null;
        }
        obj[_type] = _value;
        
        return obj;
    }

/* Method to create json of an object*/
     private static JObject getObj(JObject newJson, object i_obj)
{
    // JObject newJson = new();
    
    Type objType = i_obj.GetType();

    //FieldInfo[] 
    var objField = objType.GetFields();//.GetValue(0);//GetValue(1);
    var count = 0;
    foreach (var member in objField)
    {
        
        if (member.GetValue(i_obj) is Array){
            count++;
            // create arrayValue/mapValue

            var _json = new JObject();
            var _array = new JArray();

            _json["mapValue"] = new JObject();

            var item = (Array)member.GetValue(i_obj);
            
            foreach(var element in item){
                _json["mapValue"]["fields"] = getObj(new JObject(), (object)element);
                _array.Add(_json);
            }

            newJson[member.Name] = new JObject();
            newJson[member.Name]["arrayValue"] = new JObject();
            newJson[member.Name]["arrayValue"]["values"] = _array;
            
        } else {
            
            newJson[member.Name] = createField(member.Name, member.FieldType.ToString(), member.GetValue(i_obj));
        }
        

    }
    return newJson;
}

public static void NewWriteDoc<T>(object data, string path, string docId, PostDocCallback callback) where T : new()
    {

        var myClass =  new T();
        
        
        Type myType = myClass.GetType();
        Type t = typeof(T);
        MemberInfo[] members = myType.GetMembers(BindingFlags.Public|BindingFlags.Instance);

        
        List<string> classFields = new();// = new();
        
        for (int i =0 ; i < members.Length ; i++)
            {
                
                if (members[i].MemberType == MemberTypes.Field){
                    classFields.Add(members[i].Name);
        
                }
            // Display name and type of the member of 'MyClass'.
            
            }
        
        var dbURL = $"projects/{projectId}/databases/(default)/documents/";
        
        var json = new JObject();
        var _json = new JObject();

        _json = getObj(_json, data);
        var fieldList = getUpdateFields(data);
        
        json["fields"] = _json;
        string _updateMasks = "";
        var count = 0;
        
        var _method = "POST";
        var _path = "";
        if (docId == ""){
            _method = "POST";
            _path = $"{path}/?";
        } else {
            json["name"] = $"{dbURL}{path}/{docId}";
            foreach(var item in fieldList){
            count++;
            if (count == fieldList.Count()){
                _updateMasks += $"updateMask.fieldPaths={item}";
            } else {
                _updateMasks += $"updateMask.fieldPaths={item}&";
            }
        }
            _method = "PATCH";
            _path = $"{path}/{docId}?";
        }
        
      
        RequestHelper req = new RequestHelper {
          
            
            Uri = $"{baseUrl}/{_path}{_updateMasks}",
       
            Method = $"{_method}",
            IgnoreHttpException = true, //Prevent to catch http exceptions
            ParseResponseBody = true, //Don't encode and parse downloaded data as JSON
            ContentType = "application/json",
            EnableDebug = true,
            DefaultContentType = false,
            Headers = new Dictionary<string, string> { 
                { "Authorization", $"Bearer {authToken}" },
                // { "Accept", "application/json" },
                // { "Content-Type", "application/json"}
                },
            Params = new Dictionary<string, string>
                {
              
                    {"key", apiKey},//  FirebaseConfig.api},
                 
                },
                BodyString = json.ToString(),
        
    };
        
        if (docId == ""){
            RestClient.Post(req).Then(response => 
            { 
                callback(response.ToString());
        
            });
        } else {
            RestClient.Patch(req).Then(response => 
            { 
                callback(response.ToString());
        
            });
        }
    }

     /// <summary>
    /// Retrieves a user from the Firebase Database, given their id
    /// </summary>
    /// <param name="userId"> Id of the user that we are looking for </param>
    /// <param name="callback"> What to do after the user is downloaded successfully </param>
    public void NewGetDoc<T>(string path, string doc, GetDocCallback callback) where T : new()
    {
        // JObject json = new();
        var myClass =  new T();
        Type t = typeof(T);
        // Debug.Log(t);
        Type myType = myClass.GetType();
        MemberInfo[] members = myType.GetMembers(BindingFlags.Public|BindingFlags.Instance);

        // Get the public properties.
        PropertyInfo[] propInfos = t.GetProperties(BindingFlags.Public);
        
        // Debug.LogFormat( "\nThe public instance members of class '{0}' are : \n", myType);
        List<string> classFields = new();// = new();
        for (int i =0 ; i < members.Length ; i++)
            {
                if (members[i].MemberType == MemberTypes.Field){
                    classFields.Add(members[i].Name);
                    // Debug.LogFormat( "'{0}' is a {1}", members[i].Name, members[i].MemberType);
                }
            // Display name and type of the member of 'MyClass'.
            
            }



        RequestHelper req = new RequestHelper {
            Uri = $"{baseUrl}/{path}/{doc}",
            Method = "GET",
            IgnoreHttpException = true, //Prevent to catch http exceptions
            ParseResponseBody = true, //Don't encode and parse downloaded data as JSON
            ContentType = "application/json",
            EnableDebug = false,
            DefaultContentType = false,
            Headers = new Dictionary<string, string> { 
                { "Authorization", $"Bearer {authToken}" },
                },
            Params = new Dictionary<string, string>
                {
                    {"key", apiKey},//  FirebaseConfig.api}
                },
    };
    
        // var payload = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents/users/{userId}?key={apiKey}";
        
        RestClient.Get(req).Then(response => {
            // Waiting(500);
            JObject obj = JObject.Parse(response.Text);
            var jsonData = obj.ToObject<FirestoreDocument>();
            var fields = (JObject)obj["fields"];
            
            foreach(var field in classFields){
                json = GetJsonValue(fields, field);
            }
            callback(json);
        }).Catch(err => {
            EditorUtility.DisplayDialog ("Error", err.Message, "Ok");
        });
        
    }
    private static JObject GetJsonValue(JObject obj, string key){
        // JObject json = new();
        var value = obj[key];
        string found = "";
        var stringValue = value.SelectToken("stringValue");
        if (stringValue != null) json.Add(key, String.Format("{0}",stringValue));
        if (stringValue == null) {
            var doubleValue = value.SelectToken("doubleValue");
            if (doubleValue != null) json.Add(key, (double)doubleValue);
            if (doubleValue == null) {
                var booleanValue = value.SelectToken("booleanValue");
                if (booleanValue != null) json.Add(key, (bool)booleanValue);
                if (booleanValue == null) {
                var intValue = value.SelectToken("integerValue");
                if (intValue != null) json.Add(key, (int)intValue);
            }
            }
        }
        var arrayValue = value.SelectToken("arrayValue");
        if (arrayValue != null){
            var aa = new JArray();
            var values = arrayValue.SelectToken("values");
            var mapValue = values[0]["mapValue"];
            var fields = mapValue.Value<JObject>("fields").Properties();
            var fieldsDict = fields.ToDictionary(
                    k => k.Name,
                    v => v.Value.ToString()
                );
                JObject dd = new JObject();
            foreach (var field in fields) {
                var v = field.Value.SelectToken("stringValue");
                if (v == null) {
                    v = (double)field.Value.SelectToken("doubleValue");
                    if (v == null) {
                        v = (bool)field.Value.SelectToken("booleanValue");
                        if (v == null) {
                            v = (int)field.Value.SelectToken("integerValue");
                        }
                    }
                }
                dd.Add(field.Name, v);
            }
            aa.Add(dd);
            
            found = "found array";
            json[key] =aa;
        }
        // Debug.Log(json.ToString());
        return json;
        //String.Format("key = {0} value = {1} {2}",key, value, found);
    }

    /* end of code from other project */
    
    // Method to convert list to JArray
    private JArray ConvertListToJArray(List<object> list) {
        JArray jsonArray = new JArray();

        foreach (var item in list) {
            // Parse different types of items
            if (item is string) {
                jsonArray.Add((string)item);
            } else if (item is int) {
                jsonArray.Add((int)item);
            } else if (item is bool) {
                jsonArray.Add((bool)item);
            }
            // Add more types as needed (e.g., float)
        }

        return jsonArray;
    }
// Method to parse mapValue fields
    private Dictionary<string, object> ParseMapValue(JObject mapValue) {
        Dictionary<string, object> fields = new Dictionary<string, object>();

        foreach (var field in mapValue) {
            string fieldName = field.Key;
            JToken fieldValue = field.Value;

            // Parse different types of values
            if (fieldValue["stringValue"] != null){
                // CheckValue((JObject)fieldValue,"stringValue")) {
                fields[fieldName] = (string)fieldValue["stringValue"];
            } else if (fieldValue["integerValue"] != null) {
                fields[fieldName] = (int)fieldValue["integerValue"];
            } else if (fieldValue["booleanValue"] != null) {
                fields[fieldName] = (bool)fieldValue["booleanValue"];
            }

            
             if (fieldValue["arrayValue"] != null) {
                // If the value contains arrayValue, parse its values
                JArray arrayValue = (JArray)fieldValue["arrayValue"]["values"];
                List<object> arrayValues = ParseArrayValue(arrayValue);
                JArray array = ConvertListToJArray(arrayValues);
                // Print the parsed values
                Debug.Log("Key: " + fieldName);
                fields[fieldName] = array;
                foreach (var arrayItem in arrayValues) {
                    Debug.Log("Value: " + arrayItem.ToString());
                }
            }
            // Add more types as needed (e.g., floatValue, arrayValue, etc.)
        }

        return fields;
    }

     // Method to parse arrayValue values
    private List<object> ParseArrayValue(JArray arrayValue) {
        List<object> values = new List<object>();

        foreach (var item in arrayValue) {
            // Parse different types of values
            if (item["stringValue"] != null) {
                values.Add((string)item["stringValue"]);
            } else if (item["integerValue"] != null) {
                values.Add((int)item["integerValue"]);
            } else if (item["booleanValue"] != null) {
                values.Add((bool)item["booleanValue"]);
            }
            // Add more types as needed (e.g., floatValue)
        }

        return values;
    }

    public void ParseFirebaseJSON(JObject jsonObject){

        Dictionary<string, object> fields = new Dictionary<string, object>();
     
        Debug.Log(jsonObject.ToString());
         // Iterate through the JSON object
        foreach (var property in (JObject)jsonObject["fields"]) {
            string key = property.Key;
            JToken value = property.Value;

        
            if (value["stringValue"] != null) {
                fields[key] = (string)value["stringValue"];
            } else if (value["integerValue"] != null) {
                fields[key] = (int)value["integerValue"];
            } else if (value["booleanValue"] != null) {
                fields[key] = (bool)value["booleanValue"];
            } else if (value["doubleValue"] != null) {
                fields[key] = (double)value["doubleValue"];
            }

            if (value["mapValue"] != null) {
                // If the value contains mapValue, parse its fields
                JObject mapValue = (JObject)value["mapValue"]["fields"];
                Dictionary<string, object> nestedFields = ParseMapValue(mapValue);
                fields[key] = nestedFields;
            }

             if (value["arrayValue"] != null) {
                // If the value contains arrayValue, parse its values
                JArray arrayValue = (JArray)value["arrayValue"]["values"];
                List<object> arrayValues = ParseArrayValue(arrayValue);

                // Print the parsed values
                Debug.Log("Key: " + key);
                foreach (var arrayItem in arrayValues) {
                    Debug.Log("Value: " + arrayItem.ToString());
                }
            }
        }
        Debug.Log(ConvertDictionaryToJson(fields));
    }


   // Method to convert dictionary to JSON
    private string ConvertDictionaryToJson(Dictionary<string, object> dictionary) {
        JObject jsonObject = new JObject();

        foreach (var pair in dictionary) {
            jsonObject[pair.Key] = JToken.FromObject(pair.Value);
        }

        return jsonObject.ToString();
    }
     IEnumerator GetDocumentList() //string accessToken)
    {
        string url = $"{baseUrl}/users?key={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {authToken}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Document list retrieved successfully:");
            Debug.Log(request.downloadHandler.text);

        }
        else
        {
            Debug.LogError("Failed to retrieve document list:");
            Debug.LogError(request.error);
            Debug.Log(request.downloadHandler.text);
        }
    }

      public IEnumerator GetDocument(string collectionPath, string documentId, Action<string> onSuccess, Action<string> onFailure)
    {
        string url = $"{baseUrl}/{collectionPath}/{documentId}?key={apiKey}";
        UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
          
            onSuccess?.Invoke(request.downloadHandler.text);           
        }
        else
        {
            onFailure?.Invoke(request.error);
        }
    }


    public void NewSignIn(string email, string password, SignInCallback callback, bool signup = false)
    {
        
        Dictionary<string, string> payload = new();
        string url = "";
        int status = 0;
        if (email !="" && password !="") {
            // Debug.Log("Sign In with Email/Password");
            url = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword";
            // payload = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";
            payload["email"] = $"{email}";
            payload["password"] = $"{password}";
            payload["returnSecureToken"] = "true";
        } else {
            return;
        }
        
        payload["key"] = $"{apiKey}";
        RequestHelper req = new RequestHelper {
            Uri = $"{url}",
            Method = "POST",
            Timeout = 10,
            IgnoreHttpException = true, //Prevent to catch http exceptions
            ParseResponseBody = true, //Don't encode and parse downloaded data as JSON
            ContentType = "application/json",
            EnableDebug = false,
            DefaultContentType = false,
            Params = payload,
            Headers = new Dictionary<string, string> {
            { "Authorization", $"Bearer {authToken}" }
            },
            BodyString = "" //Serialize object using JsonUtility by default
        };

        
        
        // $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/verifyPassword?key={apiKey}"
        // RestClient.Post($"{url}?key={apiKey}",
        //     payload)
        
        RestClient.Post(req).Then(
            response =>
            {
                var responseJson = response.Text;
                var _response = JObject.Parse(responseJson);
                
                // Kept in to display error message in custom text object
                // where it parses the return error and displays the message.

                // if ()
                // var children = _response.Children().Children();
                // Waiting(500);
                
                // GameObject errorPanel = GameObject.Find("ErrorCanvas");
                
                // ErrorPanelCanvas panel = errorPanel.GetComponent<ErrorPanelCanvas>();
                
                // Dictionary<string, string> messages = new();
                // messages["INVALID_PASSWORD"] = "Invalid Password Provided.";
                // messages["EMAIL_NOT_FOUND"] = "Invalid Email Address Provided.";

                // foreach(var child in children){
                    
                //     if (child.SelectToken("message") != null){
                       
                        
                //         // Debug.Log($"{child.SelectToken("message")}");
                //         // ShowDialog<ErrorPanelCanvas>(errorPanel, $"{messages[(string)child.SelectToken("message")]}");
                //         panel.message.text = $"{messages[(string)child.SelectToken("message")]}";
                //         panel.rootCanvas.SetActive(true);
                //         // RestDBAuthErrorEvent _evt = restDBAuthErrorEvent;
             
                //         status = -1;
                //  
                //     }
                // }
                // EditorUtility.DisplayDialog("Response", responseJson, "Ok");
                // Debug.Log(responseJson);

                // Using the FullSerializer library: https://github.com/jacobdufault/fullserializer
                // its also included now with unity visual scripting
                // to serialize more complex types (a Dictionary, in this case)
                var data = fsJsonParser.Parse(responseJson);
                var _data = JObject.Parse(responseJson);
                // Debug.Log($"refreshToken = {_data["refreshToken"]}");
                object deserialized = null;
                serializer.TryDeserialize(data, typeof(Dictionary<string, string>), ref deserialized);

                var authResponse = deserialized as Dictionary<string, string>;
                //SetAuthTokenID((string)authResponse["idToken"]);
                // Debug.Log($"{AuthTokenId}");
                    // authResponse["idToken"]);

                if (signup){
                    
                    //CreateDocument("users", authResponse["localId"]);
                }
                
               // EditorUtility.DisplayDialog("Response", authResponse["mfaPendingCredential"], "Ok");
             
                currentUser.UserId = authResponse["localId"];
                currentUser.displayName = (string)_data["displayName"];
                currentUser.email = (string)_data["email"];
                currentUser.refreshToken = (string)_data["refreshToken"];
                authToken = authResponse["idToken"];
                // Debug.Log((string)_data["email"]);
               
                callback(currentUser);
                
            });
    }
}

 [Serializable]
    public class DocumentData
    {
        public string name;
        public string message;
    }

[Serializable]
public class SignInResponse
{
    public string kind;
    public string localId;
    public string email;
    public string displayName;
    public string idToken;
    public string refreshToken;
    public string expiresIn;
    public string registered;
}

