using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using dotenv.net;

namespace PaperAssistant
{
    internal class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Initialize plugins...");

            PaperAssistantPlugin paperAssistantPugin = new PaperAssistantPlugin();

            Console.WriteLine("Creating kernel...");

            var envVars = DotEnv.Read();

            IKernelBuilder builder = Kernel.CreateBuilder();

            //builder.AddOpenAIChatCompletion(
            //    "gpt-4o-mini-2024-07-18",
            //    "xxx"
            //  );

#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            builder.AddOpenAIChatCompletion(
              modelId: envVars["ToolUseModelId"],
              apiKey: envVars["ToolUseApiKey"],
              endpoint: new Uri($"{envVars["ToolUseEndpoint"]}")
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
            //builder.Plugins.AddFromObject(githubPlugin);
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
