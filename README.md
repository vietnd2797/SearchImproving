# SearchImproving
This is a simple ASP.Net Core MVC web application about searching input strings contain text on Vietnamese language and several other characters, with the combination of Duy Do's tokenizer and standard English tokenizer, using Elasticsearch.


## Overview

In this project we will combine both of the Vietnamese and English tokenizers, attempt to show the results found by the Vietnamese tokenizer on top and keep the results for each morphemes simultaneously. For example, with the keyword "phương trình bậc hai", the expected set of results has to contain (by the descending order):

- All results match exactly "phương trình bậc hai", with no change about the order of morphemes.
- Results match component words like: "phương trình", "bậc hai".
- Results match single morphemes like: "phương", "trình", "bậc", "hai".

About the dataset, in this project we will prepare a collection of questions as strings that contain both of plain text and HTML tags. All questions are imported and stored in a given database.


## Neccessary tools

- Use Elasticsearch to improve the performance of searching (version ```5.4.1```).
- Use MongoDB as a place to store data and import them to Elasticsearch.
- ASP.Net Core ```3.1```.
- Use Kibana (optional) to run test code on it if we need.
- Microsoft Visual Studio (with ASP.NET Core installed).
- Dependencies (install within Visual Studio): Elasticsearch.Net ```5.4.0```, Nest ```5.4.0```, Newtonsoft.Json ```10.0.1```, MongoDB.Bson ```2.11.2``` and MongoDB.Driver ```2.11.2```. 



