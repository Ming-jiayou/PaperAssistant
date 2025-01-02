# 使用C#构建一个论文总结AI Agent

## 前言

我觉得将日常生活中一些简单重复的任务交给AI Agent，是学习构建AI Agent应用一个很不错的开始。本次分享我以日常生活中一个总结论文的简单任务出发进行说明，希望对大家了解AI Agent有所帮助。任务可以是多种多样的，真的帮助自己提升了效率，那就是一个很不错的开始了！！

我的这个简单任务是这样的，有一篇文献，如下所示：

![image-20250102100353781](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102100353781.png)

我想要对该文献进行总结，然后将md格式笔记保存。

我之前的做法是使用Cherry Studio新建一个论文总结助手，如下所示：

![image-20250102100813485](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102100813485.png)

然后上传文献，进行总结，如下所示：

![image-20250102101204179](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102101204179.png)

然后新建一个笔记md文件，将这些内容复制进去，这样就完成了一个简单的任务，如下所示：

![image-20250102101513430](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102101513430.png)

虽然说已经比最开始的时候，只是将文献翻译一下，就直接开始读，然后尝试自己总结主要内容强太多了。但是还是有需要改进的地方，那就是选择文件，新建笔记文件，复制笔记内容这些简单重复的事，可以尝试一下把这些交给一个AI Agent！！

## 使用C#构建一个论文总结AI Agent相关实践

前几个月，当我刚开始尝试构建AI Agent应用的时候，经过测试，我发现在Semantic Kernel中，想要使用函数调用的话，只有OpenAI与Kimi的模型能用，而OpenAI模型的使用在国内是不太方便的，而构建一个AI Agent函数调用功能是必不可少的。经过一番探索，找到了一位大佬的方法，可以通过提示词来实现函数调用：

[Semantic Kernel/C#：一种通用的Function Calling方法，文末附大模型清单](https://mp.weixin.qq.com/s/ZYIQeyYs8YlCkM3g82tChA)

然后根据这个方法，做了一个简单的AI Agent项目进行介绍：

[SimpleAIAgent：使用免费的glm-4-flash即可开始构建简单的AI Agent应用](https://mp.weixin.qq.com/s/Doz2szH00hDysvn6tFZKEQ)

GitHub地址：https://github.com/Ming-jiayou/SimpleAIAgent

经过几个月的发展，我发现现在在Semantic Kernel中使用国内具有函数调用能力的模型效果也还行了。现在开始构建我们自己的AI Agent应用吧！！

为了尽量保持简单，不增加无关的心智负担，便于感兴趣的朋友自己动手，新建一个C#控制台项目。

实现这个简单的Demo可以有五种不同的方式：

![image-20250102103959346](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102103959346.png)

第一种使用基本的Semantic Kernel中的Function calling with chat completion

相关文档：[Function calling with chat completion | Microsoft Learn](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/?pivots=programming-language-csharp)

第二种使用Semantic Kernel Chat Completion Agent

相关文档：[Exploring the Semantic Kernel Chat Completion Agent (Experimental) | Microsoft Learn](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/chat-completion-agent?pivots=programming-language-csharp)

第三种使用Microsoft.Extensions.AI

相关文档：[extensions/src/Libraries/Microsoft.Extensions.AI.OpenAI at main · dotnet/extensions](https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.AI.OpenAI)

第四种使用Semantic Kernel Open AI Assistant Agent

相关文档：[Exploring the Semantic Kernel Open AI Assistant Agent (Experimental) | Microsoft Learn](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/assistant-agent?pivots=programming-language-csharp)

第五种使用UniversalLLMFunctionCaller

相关文档：[Jenscaasen/UniversalLLMFunctionCaller: A planner that integrates into Semantic Kernel to enable function calling on all Chat based LLMs (Mistral, Bard, Claude, LLama etc)](https://github.com/Jenscaasen/UniversalLLMFunctionCaller)

我先使用第二种方式进行说明。

先安装这三个库：

![image-20250102105224219](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102105224219.png)

实现这个AI Agent需要自己先写好一个总结论文的相关插件：

最初的插件：

```csharp
 [KernelFunction("ExtractPDFContent")]
 [Description("读取指定路径的PDF文档内容")]
 [return: Description("PDF文档内容")]
 public string ExtractPDFContent(string filePath)
 {
     StringBuilder text = new StringBuilder();
     // 读取PDF内容
     using (PdfDocument document = PdfDocument.Open(filePath))
     {
         foreach (var page in document.GetPages())
         {
             text.Append(page.Text);
         }
     }
     return text.ToString();
 }

  [KernelFunction]
  [Description("根据文件路径与笔记内容创建一个md格式的文件")]
  public void SaveMDNotes([Description("保存笔记的路径")] string filePath, [Description("笔记的md格式内容")] string mdContent)
  {
      try
      {
          // 检查文件是否存在，如果不存在则创建
          if (!File.Exists(filePath))
          {
              // 创建文件并写入内容
              File.WriteAllText(filePath, mdContent);
          }
          else
          {
              // 如果文件已存在，覆盖写入内容
              File.WriteAllText(filePath, mdContent);
          }
      }
      catch (Exception ex)
      {
          // 处理异常
          Console.WriteLine($"An error occurred: {ex.Message}");
      }
  }
```

原本是想AI自己多次调用这些函数，比如先调用第一个获取pdf文献内容，然后生成一个md格式笔记，然后再调用第二个函数。但是在实际实践中，只有OpenAI的模型这样子效果还可以，其他的模型多次函数调用的效果并不好，因此最终选择内置一个Kernel的方法。

最终的插件：

```csharp
    internal sealed class PaperAssistantPlugin
    {
        public PaperAssistantPlugin() 
        {
            IKernelBuilder builder = Kernel.CreateBuilder();
#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            builder.AddOpenAIChatCompletion(
              modelId: "deepseek-ai/DeepSeek-V2.5",
              apiKey: "sk-xxx",
              endpoint: new Uri("https://api.siliconflow.cn")
            );         
#pragma warning restore SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            InterKernel = builder.Build();
        }

        internal Kernel InterKernel { get; set; }

        [KernelFunction("ExtractPDFContent")]
        [Description("读取指定路径的PDF文档内容")]
        [return: Description("PDF文档内容")]
        public string ExtractPDFContent(string filePath)
        {
            StringBuilder text = new StringBuilder();
            // 读取PDF内容
            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                foreach (var page in document.GetPages())
                {
                    text.Append(page.Text);
                }
            }
            return text.ToString();
        }

        [KernelFunction]
        [Description("根据文件路径与笔记内容创建一个md格式的文件")]
        public void SaveMDNotes([Description("保存笔记的路径")] string filePath, [Description("笔记的md格式内容")] string mdContent)
        {
            try
            {
                // 检查文件是否存在，如果不存在则创建
                if (!File.Exists(filePath))
                {
                    // 创建文件并写入内容
                    File.WriteAllText(filePath, mdContent);
                }
                else
                {
                    // 如果文件已存在，覆盖写入内容
                    File.WriteAllText(filePath, mdContent);
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        [KernelFunction]
        [Description("总结论文内容生成一个md格式的笔记，并将笔记保存到指定路径")]      
        public async void GeneratePaperSummary(string filePath1,string filePath2)
        {
            StringBuilder text = new StringBuilder();
            // 读取PDF内容
            using (PdfDocument document = PdfDocument.Open(filePath1))
            {
                foreach (var page in document.GetPages())
                {
                    text.Append(page.Text);
                }
            }

            // 生成md格式的笔记
            string skPrompt = """
                                论文内容：

                                {{$input}}

                                请总结论文的摘要、前言、文献综述、主要论点、研究方法、结果和结论。
                                论文标题为《[论文标题]》，作者为[作者姓名]，发表于[发表年份]。请确保总结包含以下内容：
                                论文摘要
                                论文前言
                                论文文献综诉
                                主要研究问题和背景
                                使用的研究方法和技术
                                主要结果和发现
                                论文的结论和未来研究方向
                                """;
            var result = await InterKernel.InvokePromptAsync(skPrompt, new() { ["input"] = text.ToString() });

            try
            {
                // 检查文件是否存在，如果不存在则创建
                if (!File.Exists(filePath2))
                {
                    // 创建文件并写入内容
                    File.WriteAllText(filePath2, result.ToString());
                    Console.WriteLine($"生成笔记成功，笔记路径：{filePath2}");
                }
                else
                {
                    // 如果文件已存在，覆盖写入内容
                    File.WriteAllText(filePath2, result.ToString());
                }
            }
            catch (Exception ex)
            {
                // 处理异常
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}

```

内置的一个Kernel用于生成md格式的论文笔记。

主函数如下所示：

```csharp
    internal class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Initialize plugins...");

            PaperAssistantPlugin paperAssistantPugin = new PaperAssistantPlugin();

            Console.WriteLine("Creating kernel...");
            IKernelBuilder builder = Kernel.CreateBuilder();

            //builder.AddOpenAIChatCompletion(
            //    "gpt-4o-mini-2024-07-18",
            //    "xxx"
            //  );

#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            builder.AddOpenAIChatCompletion(
              modelId: "Qwen/Qwen2.5-32B-Instruct",
              apiKey: "xxx",
              endpoint: new Uri("https://api.siliconflow.cn")
            );

            // builder.AddOpenAIChatCompletion(
            //  modelId: "glm-4-flash",
            //  apiKey: "xxx",
            //  endpoint: new Uri("https://open.bigmodel.cn/api/paas/v4")
            //);

            // builder.AddOpenAIChatCompletion(
            //  modelId: "yi-large-fc",
            //  apiKey: "xxx",
            //  endpoint: new Uri("https://api.lingyiwanwu.com/v1")
            //);
#pragma warning restore SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            builder.Plugins.AddFromObject(paperAssistantPugin);

            Kernel kernel = builder.Build();

            Console.WriteLine("Defining agent...");
#pragma warning disable SKEXP0110 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            ChatCompletionAgent agent =
                new()
                {
                    Name = "PaperAssistantAgent",
                    Instructions =
                            """
                             你是一个用于读取pdf文献内容，并总结内容，生成一个md笔记的智能代理。
                             用户提供论文路径与创建笔记的路径
                             注意文件路径的格式应如下所示：
                             "D:\文献\表格识别相关\文献\xx.pdf"
                             "D:\文献\表格识别相关\笔记\xx.md"
                             请总结论文的摘要、前言、文献综述、主要论点、研究方法、结果和结论。
                             论文标题为《[论文标题]》，作者为[作者姓名]，发表于[发表年份]。请确保总结包含以下内容：
                             论文摘要
                             论文前言
                             论文文献综诉
                             主要研究问题和背景
                             使用的研究方法和技术
                             主要结果和发现
                             论文的结论和未来研究方向
                             """,
                    Kernel = kernel,
                    Arguments =
                        new KernelArguments(new OpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),MaxTokens = 16000})                     
                };
#pragma warning restore SKEXP0110 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            Console.WriteLine("Ready!");

            ChatHistory history = [];
            bool isComplete = false;
            do
            {
                Console.WriteLine();
                Console.Write("> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                {
                    isComplete = true;
                    break;
                }
                if (input.Trim().Equals("Clear", StringComparison.OrdinalIgnoreCase))
                {
                    history.Clear();
                    Console.WriteLine("已清除聊天记录");
                    continue;
                }

                history.Add(new ChatMessageContent(AuthorRole.User, input));

                Console.WriteLine();
              
#pragma warning disable SKEXP0110 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
                await foreach (ChatMessageContent response in agent.InvokeAsync(history))
                {
                    // Display response.
                    Console.WriteLine($"{response.Content}");
                }
#pragma warning restore SKEXP0110 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            } while (!isComplete);
        }
    }
}
```

在主函数中定义的这个Kernel主要用于与用户交互与选择调用哪个函数。

开始查看效果：

![image-20250102110835592](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102110835592.png)

会发现在自动调用插件中的这个函数了。

![image-20250102110931834](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102110931834.png)

由于我使用的是异步方式，AI会先给出回答，实际上笔记还没有真的生成，当出现生成笔记成功，笔记路径：xxx的时候，笔记才真的生成成功，如下所示：

![image-20250102111211988](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102111211988.png)

![image-20250102111135929](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102111135929.png)



![image-20250102111303999](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102111303999.png)

就成功实现我们自己的简单的AI Agent应用了。

## 直接使用

可能很多人并不熟悉C#也不太懂得编程，但是对自己构建AI Agent应用还是很感兴趣的。接下来我将手把手地介绍该如何使用，希望完全的小白也能学会使用。

项目GitHub地址：https://github.com/Ming-jiayou/PaperAssistant

C#程序员git clone项目之后，将.env.example改为.env然后填入自己想要使用的模型与密钥以及Endpoint即可。

这里重点介绍一下非程序朋友的使用。

我已经发布了一个版本放到GitHub上了，如果上GitHub有问题，也可以联系我。但还是推荐从GitHub上下载，比较安全一点，随便打开别人给的文件不太好。

![image-20250102163753555](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102163753555.png)

![image-20250102163814932](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102163814932.png)

下载好了之后，解压如下所示：

![image-20250102163832037](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102163832037.png)

将.env.example改为.env然后填入自己想要使用的模型与密钥以及Endpoint即可。

![image-20250102163925711](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102163925711.png)

以下是手把手的尝试几个不同的平台。

**SiliconCloud**

现在注册有送2000万token的活动，最nice的一点是送的token没有时间期限。想试试的朋友可以点击链接：https://cloud.siliconflow.cn/i/Ia3zOSCU，注册使用。

![image-20250102144700195](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102144700195.png)

直接点击exe文件，即可使用：

![image-20250102144800507](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102144800507.png)

出现完成之后，并没有真的完成：

![image-20250102144927790](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102144927790.png)

需要继续等待，等到出现“生成笔记成功，笔记路径：xxx”的时候才真正生成完成：

![image-20250102145051958](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102145051958.png)

![image-20250102145743037](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102145743037.png)

**智谱AI**

glm-4-flash是免费使用的，配置如下所示：

![image-20250102145436991](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102145436991.png)

效果：

![image-20250102145801666](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102145801666.png)

![image-20250102145841211](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102145841211.png)

可以发现总结的内容比Qwen/Qwen2.5-72B-Instruct-128K要少很多。

**DeepSeek**

配置如下所示：

![image-20250102150451237](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102150451237.png)

效果：

![image-20250102150712209](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102150712209.png)

第一次没成功，就再来一次。

![image-20250102150939008](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102150939008.png)

总结的很好，可惜是英文的，再试一次：

![image-20250102151237085](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102151237085.png)

总结的也很不错，但是不好的一点是似乎陷入了死循环：

![image-20250102151336922](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102151336922.png)

过一会又把生成的覆盖掉了，变成英文的了，如下所示：

![image-20250102151457836](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102151457836.png)

一下就花掉了10万token，我这个应用看来不适合使用DeepSeek。

![image-20250102151703421](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102151703421.png)

**零一万物**

配置如下：

![image-20250102152515824](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102152515824.png)

需要改成yi-large-fc才行。

效果：

![image-20250102152922071](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102152922071.png)

![image-20250102153014272](https://mingupupup.oss-cn-wuhan-lr.aliyuncs.com/imgs/image-20250102153014272.png)

如果有朋友硅基流动赠送的额度都用完了，但是也想体验一下，可以联系我获取体验的api key，用的量多了随时关闭，不太可靠。
