using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics;
using UglyToad.PdfPig;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using dotenv.net;

namespace PaperAssistant
{
    internal sealed class PaperAssistantPlugin
    {
        public PaperAssistantPlugin() 
        {
            var envVars = DotEnv.Read();

            IKernelBuilder builder = Kernel.CreateBuilder();
#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            builder.AddOpenAIChatCompletion(
              modelId: envVars["PaperSummaryModelId"],
              apiKey: envVars["PaperSummaryApiKey"],
              endpoint: new Uri($"{envVars["PaperSummaryEndpoint"]}")
            );
            // builder.AddOpenAIChatCompletion(
            //  modelId: "Qwen/Qwen2.5-72B-Instruct-128K",
            //  apiKey: "27077583e5ea767c1814bef784addbfc.E8IcjmAR8evERtza",
            //  endpoint: new Uri("https://open.bigmodel.cn/api/paas/v4")
            //);
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
