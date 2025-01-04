using dotenv.net;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace PaperAssistantB
{
    internal class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Initialize plugins...");
            PaperAssistantPlugin paperAssistantPlugin = new PaperAssistantPlugin();

            var envVars = DotEnv.Read();
            ApiKeyCredential apiKeyCredential = new ApiKeyCredential(envVars["ToolUseApiKey"]);

            OpenAIClientOptions openAIClientOptions = new OpenAIClientOptions();
            openAIClientOptions.Endpoint = new Uri($"{envVars["ToolUseEndpoint"]}");

            IChatClient openaiClient =
            new OpenAIClient(apiKeyCredential, openAIClientOptions)
            .AsChatClient(envVars["ToolUseModelId"]);

            IChatClient client = new ChatClientBuilder(openaiClient)
            .UseFunctionInvocation()
            .Build();


            ChatOptions chatOptions = new()
            {
                Tools = [AIFunctionFactory.Create(paperAssistantPlugin.ExtractPDFContent),
                         AIFunctionFactory.Create(paperAssistantPlugin.SaveMDNotes),
                         AIFunctionFactory.Create(paperAssistantPlugin.GeneratePaperSummary)]
            };

            string skPrompt = """
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
                             """;
            List<ChatMessage> history = [];
            history.Add(new ChatMessage(ChatRole.System, skPrompt));
            bool isComplete = false;
            Console.WriteLine("Ready!");
            do
            {
                Console.WriteLine();
                Console.Write("> ");
                string? input = Console.ReadLine();
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

                history.Add(new ChatMessage(ChatRole.User, input));

                Console.WriteLine();

                var result = await client.CompleteAsync(input, chatOptions);

                Console.WriteLine(result.ToString());

                // Add the message from the agent to the chat history
                history.Add(new ChatMessage(ChatRole.Assistant, result.ToString() ?? string.Empty));
            } while (!isComplete);
        }
    }
}
