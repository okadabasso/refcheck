# refcheck

ソリューション内にあるC#プロジェクトファイルをスキャンし、循環参照を検出する。  

ソリューション内にあるプロジェクトの出力アセンブリを他のプロジェクトの参照に追加してたりすると意図せず循環参照が起こってしまうので、回避の施策をとるためのツール。
出力は Markdown にしてあるので html化して読むこともできる。

## build

特に参照オブジェクトもないので以下のようにするかVisualStudio2013でソリューションを開きビルドする。

```
C:.\..\ > msbuild refcheck\refcheck.csproj
```


## usage

```
C:.\..\ > refcheck.exe solution_dir
```

## 出力例

```
## project references .

### project SampleApp

* assembly: System
* assembly: System.Core
* assembly: System.Xml.Linq
* assembly: System.Data.DataSetExtensions
* assembly: System.Data
* assembly: SampleApp.Database
* assembly: SampleApp.Models
* assembly: SampleApp.Forms

### project SampleApp.Database

* assembly: System
* assembly: System.Core
* assembly: System.Xml.Linq
* assembly: System.Data.DataSetExtensions
* assembly: System.Data
* assembly: SampleApp.Models


### project SampleApp.Models

* assembly: System
* assembly: System.Core
* assembly: System.Xml.Linq
* assembly: System.Data.DataSetExtensions
* assembly: System.Data
* assembly: SampleApp.Database
* assembly: SampleApp.Forms

### project SampleApp.Forms

* assembly: System
* assembly: System.Core
* assembly: System.Xml.Linq
* assembly: System.Data.DataSetExtensions
* assembly: System.Data
* assembly: System.Windows.Forms
* assembly: SampleApp.Models

## references check .

* SampleApp
    * SampleApp.Database
        * SampleApp.Models
            * SampleApp.Database __circular reference__
    * SampleApp.Models
        * SampleApp.Database
            * SampleApp.Models __circular reference__
    * SampleApp.Forms
        * SampleApp.Models

```

循環参照を検出したら  __circular reference__ と出力する。
