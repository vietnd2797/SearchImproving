# SearchImproving
This is a simple ASP.Net Core MVC web application about searching input strings contain text on Vietnamese language and several other characters, with the combination of Duy Do's tokenizer and standard English tokenizer, using Elasticsearch.

## Documentation
### Abstract

Nowadays, the searching problem on Vietnamese language is not too much strange with us and there are a variety of Vietnamese searching tools to support. The fundamental element in searching problem is tokenizer, which demarcates and classifies words in a given input string. Operation of a tokenizer is dependent to the lexicon of the language that is considered. Hence, we could aware easily the differences between Vietnamese tokenizer and English tokenizer. The Vietnamese tokenizer segments the string to tokens in which it keeps Vietnamese polymorphemic words (Vietnamese: từ ghép) and the English tokenizer only segments to morphemes, if we use it to a string that has text on Vietnamese language, since it does not understand Vietnamese language. Oppositely, if we search the input string is a morpheme, the Vietnamese tokenizer could find none of results, since the input  string does not fit with the token that is tokenized (typically, is polymorphemic words). For illustration, we consider a simple example about searching on Elasticsearch:

```js
GET questions/_search
{
  "query": {
    "match": {
      "content": "mệnh đề"
    }
  }
}
```
For Elasticsearch, we could find the usage via the document on: https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html

In this example, we suppose the Vietnamese tokenizer existed and a dataset was imported to Elasticsearch. In the case of this above code block, we retrieve the output:

```js
{
  "took": 3,
  "timed_out": false,
  "_shards": {
    "total": 1,
    "successful": 1,
    "failed": 0
  },
  "hits": {
    "total": 49,
    ...
 ```
 such we have 49 results that are found. However, if we change the input string from "mệnh đề" to "mệnh", we have none of results:
 
 ```js
 {
  "took": 0,
  "timed_out": false,
  "_shards": {
    "total": 1,
    "successful": 1,
    "failed": 0
  },
  "hits": {
    "total": 0,
    "max_score": null,
    "hits": []
  }
}
```
The cause for this case is the Elasticsearch search results by tokenization for each document that was indexed, hence it will search all results matched the token "mệnh đề" from the Vietnamese tokenizer we used, not "mệnh".

### Purpose and expected result

In this project we will combine both of the Vietnamese and English tokenizers, attempt to show the results found by the Vietnamese tokenizer on top and keep the results for each morphemes simultaneously. For example, with the keyword "phương trình bậc hai", the expected set of results has to contain (by the descending order):

- All results match exactly "phương trình bậc hai", with no change about the order of morphemes.
- Results match component words like: "phương trình", "bậc hai".
- Results match single morphemes like: "phương", "trình", "bậc", "hai".

About the dataset, in this project we will prepare a collection of 847 questions as strings that contain both of plain text and HTML tags. All of questions are imported and stored in a given database, such we just need to get them from database, added them into indices that is created in Elasticsearch and operate with ASP.Net Core (C# language) via dependencies such as Elasticsearch.Net and Nest.


### Neccessary tools

- Use Elasticsearch to improve the performance of searching (Good for the version ```5.4.1```)
- Use MongoDB as a place to store data and import them to Elasticsearch.
- ASP.Net Core ```3.1```
- Dependencies for project: Elasticsearch.Net ```5.4.0```, Nest ```5.4.0```, Newtonsoft.Json ```10.0.1```
- Use Kibana (optional) to run test code on it if we need.
 

### Solving method
- Use Vietnamese tokenizer to get results match polymorphemic words.
- Use standard tokenizer (probably for English) to get results match single morphemes.  
- Eliminate coincident results.

### Installation of neccessary tools


#### Elasticsearch and Kibana


In this project, we use the version ```5.4.1``` of Elasticsearch, not the latest. We will explain the cause of this problem in the following section. For installation, we need to access to https://www.elastic.co/fr/downloads/past-releases/elasticsearch-5-4-1, and download (for compatible format of the file with OSs, we could check on https://www.elastic.co/guide/en/elasticsearch/reference/current/install-elasticsearch.html). After that, extract the file that was downloaded to a specified folder (note that we should not extract to a folder inside a disk that has too low remaining memory).

After extracting, now we have the folder ```elasticsearch-5.4.1```. Then, go to the folder ```bin``` and run the file ```elasticsearch.bat```, or open the terminal of your OS (in this project, the OS is Windows) and use the following command to run the service (in this case, the folder ```elasticsearch-5.4.1``` is located in ```C:```):
```js
$ C:\elasticsearch-5.4.1\bin>elasticsearch.bat
```
and check the result by using the command:
```js
$ curl -X GET "localhost:9200"
{
  "name" : "0_XE52p",
  "cluster_name" : "elasticsearch",
  "cluster_uuid" : "qHO930FXQLa6IOVOObW9DA",
  "version" : {
    "number" : "5.4.1",
    "build_hash" : "2cfe0df",
    "build_date" : "2017-05-29T16:05:51.443Z",
    "build_snapshot" : false,
    "lucene_version" : "6.5.1"
  },
  "tagline" : "You Know, for Search"
}
```
or access http://localhost:9200 on your web browser. Note that our version of Elasticsearch requires the appropriate version of Java. In the case of the version ```5.4.1```, I use the ```1.8.0_261``` Java version. For installation of Java, we could download file from https://www.oracle.com/java/technologies/javase-downloads.html. After installing successfully, we need to add environment variables. Specifically, if our OS is Windows, in ```System variables```, we create a ```JAVA_HOME``` variable with its value is our Java path and add the path to the folder ```bin``` to ```Path```, then use the command:
```js
java -version
```
to check. If the Java version is not compatible, when running the Elasticsearch service, we will encounter the following message (in this case I run Elasticsearch ```7.3.1``` with the old Java ```1.8.0_261```):
```js
future versions of Elasticsearch will require Java 11; your Java version from [D:\ProgramFiles\Java\jdk1.8.0_261\jre] does not meet this requirement
```

Beside Elasticsearch, Kibana is the accompanying tool that supplies an IDE that allows coding and running Elasticsearch code on it. We could use Kibana like a supported tool to test. For installation of Kibana, we need to access to https://www.elastic.co/fr/downloads/past-releases/kibana-5-4-1 and choose a file that is compatible with your OS. Note that the version of Kibana have to be coincident with the version of Elasticsearch. Then we extract the file to a specified folder, so conveniently, we should extract to the folder that contains the folder ```elasticsearch-5.4.1```. Next we run the file ```kibana.bat``` in the folder ```bin```, or use the following command:
```js
$ C:\kibana-5.4.1-windows-x86\bin>kibana.bat
```
and check by accessing to http://localhost:5601 on your web browser.

Although the latest version of Elasticsearch and Kibana is 7.9.2, in this project we use the version 5.4.1. We will explain the cause of this problem in the following section.

#### Vietnamese tokenizer

The plugin ```elasticsearch-analysis-vietnamese``` of the author Duy Do (details on https://github.com/duydo/elasticsearch-analysis-vietnamese) is a quite notable plugin that allows integrating Vietnamese language into Elasticsearch and attracts a large amount of users. In this project, we will use it such for installation of it, we need to install Elasticsearch previously. There are three ways to build the plugin with Elasticsearch: Build from source, build by using docker and build directly by using the file ```elasticsearch-plugin.bat``` in the folder ```bin```.
- For building from source, we could check this post written by the author Duy Do here: https://duydo.me/how-to-build-elasticsearch-vietnamese-analysis-plugin/. We need to have a basic knowledge about the installation and the usage of Maven to build. I attempted to build by this way, but failed in the fourth step because of the failure in the addition of dependencies. If you are a beginner, I suggest you should not build by this way.

- For building by using docker, we could check the post written by the author Le Xuan Duy here: https://viblo.asia/p/elasticsearch-phan-tich-va-tim-kiem-du-lieu-tieng-viet-3P0lPveoKox if we had knowledge about docker.

- For building by using ```elasticsearch-plugin.bat```, we will find on https://github.com/duydo/elasticsearch-analysis-vietnamese/releases the release that is compatible with our version of Elasticsearch, then download and extract it. After that, start the Elasticsearch service, and go to the folder ```bin``` that is located in the folder ```elasticsearch-x.x.x``` and run the following command (recall, in this project I am using the version ```5.4.1``` of Elasticsearch):

```js 
$ C:\elasticsearch-5.4.1\bin>elasticsearch-plugin install file:\\\path\elasticsearch-analysis-vietnamese-5.4.1.zip
```
Then we need to close the Elasticsearch and restart it in order to reload the plugin. Because of the simplicity of this way, such I decide to use and suggest it to beginners. 

After installing successfully, we could check the operation of the plugin by creating an index as follow aa (In this case I create an index with the intended object is question with 3 fields: timestamp, id, content and the tokenizer name that is used):
```js
PUT question_index
{    
  "settings":
  {
    "index":
    {
      "number_of_shards" : 1,
      "number_of_replicas" : 0
    },
    "analysis":
    {
      "analyzer":
      {
        "my_analyzer":
        {
          "type" : "custom",
          "tokenizer": "vi_tokenizer",
          "char_filter":  "html_strip",
          "filter": ["lowercase"]
        }
      }
    }
  },
  "mappings":
  {
    "properties":
    {
      "@timestamp": 
      {
        "type": "text"
      },
      "id":
      {
        "type": "integer"
      },
      "content":
      {
        "type": "text",
        "analyzer": "my_analyzer"
      },
      "tokenizer": 
      {
        "type": "text"
      }
    }
  }
}
```
If we use Elasticsearch ```5.4.1```, we have to set the code block ```mapping``` into a new code block that represents an object that owns this mapping, like ```"info"```. Then we retrieve the result:
```js
{
  "acknowledged" : true,
  "shards_acknowledged" : true,
  "index" : "question_index"
}
```
After that we add a document to the index that have just created:
```js
POST _bulk
{"index" : {"_index": "question_index", "_id": 1}}
{"@timestamp": "08/10/2020 9:01:00 am", "id": 1, "content": "<p>Trong các mệnh đề sau, mệnh đề nào đúng ?</p>", "tokenizer": "vi_tokenizer"}
```
If we use Elasticsearch ```5.4.1```, we have to add ```"type": "doc"``` into ```"index"```. If the addition is successfully, we will retrieve the following result


##### Why we use Elasticsearch 5.4.1 in this project?

As we argument above, although the latest version of Elasticsearch is ```7.9.2``` at the present, but in this project we use the version ```5.4.1```, because when I use Elasticsearch ```7.3.1``` (therefore I have to use Kibana and Duy Do's plugin with this version respectively), I am unable to add a document (specifically, the question) with the field value has HTML tags. Such that with my usage of ```POST _bulk``` above, I have got the following message:
```js
{
  "took" : 12,
  "errors" : true,
  "items" : [
    {
      "index" : {
        "_index" : "question_index",
        "_type" : "_doc",
        "_id" : "1",
        "status" : 500,
        "error" : {
          "type" : "string_index_out_of_bounds_exception",
          "reason" : "String index out of range: -1"
        }
      }
    }
  ]
}
```
and have attempted to fix this problem but it is inefficient. Therefore I have to downgrade the version to ```5.4.1```.


#### MongoDB 

For installation of MongoDB, we could check on https://www.mongodb.com/try/download/community to download. The MongoDB Community Server is the appropriate choice, sufficient to use for the small scope. After installing successfully, start the service by running the command:
```js
$ path\MongoDB\Server\4.4\bin>mongo.exe
```
then we could start to add databases and collections with MongoDB. The default host of MongoDB is http://localhost:27017. Beside this way we could connect to host on MongoDB Compass that is available when installing, and operate on it.

We also check the detail of installation guide on https://docs.mongodb.com/manual/installation/.






