using dotenv.net;
using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace PaperAssistantB
{
    internal sealed class PaperAssistantPlugin
    {
        public PaperAssistantPlugin()
        {
            var envVars = DotEnv.Read();
            ApiKeyCredential apiKeyCredential = new ApiKeyCredential(envVars["PaperSummaryApiKey"]);

            OpenAIClientOptions openAIClientOptions = new OpenAIClientOptions();
            openAIClientOptions.Endpoint = new Uri($"{envVars["PaperSummaryEndpoint"]}");

            IChatClient openaiClient =
            new OpenAIClient(apiKeyCredential, openAIClientOptions)
                .AsChatClient(envVars["PaperSummaryModelId"]);

            Client = new ChatClientBuilder(openaiClient)
                         .UseFunctionInvocation()
                         .Build();
        }

        internal IChatClient Client { get; set; }

        [Description("读取指定路径的PDF文档内容")]
        [return: Description("PDF文档内容")]
        public string ExtractPDFContent(string filePath)
        {
            Console.WriteLine($"执行函数ExtractPDFContent，参数{filePath}");

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

        [Description("根据文件路径与笔记内容创建一个md格式的文件")]
        public void SaveMDNotes([Description("保存笔记的路径")] string filePath, [Description("笔记的md格式内容")] string mdContent)
        {
            try
            {
                Console.WriteLine($"执行函数SaveMDNotes，参数1：{filePath},参数2：{mdContent}");

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

        [Description("总结论文内容生成一个md格式的笔记，并将笔记保存到指定路径")]
        public async void GeneratePaperSummary(string filePath1, string filePath2)
        {
            Console.WriteLine($"执行函数GeneratePaperSummary，参数1：{filePath1},参数2：{filePath2}");

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
                                请使用md格式总结论文的摘要、前言、文献综述、主要论点、研究方法、结果和结论。
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
            history.Add(new ChatMessage(ChatRole.User, text.ToString()));

            var result = await Client.CompleteAsync(history);

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
