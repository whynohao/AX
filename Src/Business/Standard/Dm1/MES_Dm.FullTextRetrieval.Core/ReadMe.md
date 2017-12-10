 全文检索功能模块
====================

### 索引管理
*  通过索引管理工厂，创建索引管理类。
	```C#
	//创建索引管理者
	IIndexManager indexManager = IndexManagerFactory.CreateIndexManager();
	```
*  索引管理者有三个功能，分别是添加索引、删除索引、查询索引。
添加索引
	```C#
	//数据可以从数据库等读取
    AbstractFileBase fileBase = new TextFileInfo() {
		FileId="1", 
		CreateTime="20161201", 
		FileName= "新建文本文档.txt" , 
		FilePath = "C:/TestDoc", 
		UpLoadPersonId = "zhangsan", 		
		Content=File.ReadAllText(Path.Combine("C:/TestDoc", "新建文本文档.txt"))};
	indexManager.CreateIndex(fileBase);
	```
	删除索引
	```C#
	//最好给每个索引有一个唯一的值
	indexManager.DeleteIndex(new TextFileInfo() { FileId = "1" });

	```
	查询索引
	```C#
	Dictionary<string, string> dic = new Dictionary<string, string>();
    dic.Add("content", "大家");
	    SearchResult searchResult = (SearchResult)indexManager.SearchIndex(dic, 1, 10);
	```
   **SearchResult** 这个就是查询结果对象。

### 关键字高亮处理
* 通过高亮处理者工厂，创建高亮处理者
```C#
	IHightLighter hightLighter = HightLighterFactory.CreateHightLighter(); //得到一个关键字高亮处理者
```
* 将查询出来的**SearchResult**对象中的AbstractFileBase，和输入关键字字典传递给**InitHightLight**函数
```C#
	AbstractFileBase fileBase = ... 
	Dictionary<string, string> dic = new Dictionary<string, string>();
	dic.Add("content", "大家");
	fileBase = hightLighter.InitHightLight(dic, fileBase);
```
* 默认内容会被关键字高亮处理，也可以自己设置
```C#
	List<HightLightField> hightLightFieldsList = new List<HightLightField>();
	hightLightFieldsList.Add(HightLightField.FileName);//将文件名设置也成为关键字高亮
	hightLighter.SetHightLightFields(list);
```