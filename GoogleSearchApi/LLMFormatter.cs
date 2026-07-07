using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroqSharp;
using System.Threading.Tasks;
using GroqSharp.Models;

namespace GoogleSearchApi
{
    class LLMFormatter
    {
        public class MessageDto
        {
            
            public string Text { get; set; }
            public ICollection<OutputJson> Data { get; set; }
        }
        public class OutputJson
        {
            public string JsonField { get; set; }
        }
        public static async Task<string> FormatMessage(MessageDto request)
        {
            var apiKey = "your-groq-api-key";
            var apiModel = "llama-3.3-70b-versatile";

            IGroqClient groqClient = new GroqClient(apiKey, apiModel)
               .SetTemperature(0.2)  // randomness of response   0 = more deterministic , 1= random
               .SetMaxTokens(256) // limits the output length
               .SetTopP(1) //
               .SetStop("NONE"); // tells the model when to stop 
                                 // var dbResponse = await _textTokenizer.GetFinalResponse(request);
                                 // var matchedKeywords = await _textTokenizer.GetMatchedStrings(request);
            var prompt = new StringBuilder();
            var question = request.Text;
            var data = request.Data;
            prompt.AppendLine($"User's Question: {question}");
            prompt.AppendLine($"Extract the following fields in JSON format:{data}");
            var response = await groqClient.CreateChatCompletionAsync(
             new Message
             {
                 Role = MessageRoleType.System,
                 Content =
                     "You are a data extraction assistant. \\\r\nYour task is to extract structured datapoints from the user input. \\\r\nIdentify the fields and values mentioned in the input and return them as a JSON array of objects. \\\r\nAlways use the field names exactly as given or implied in the input. \\\r\nIf multiple records are present, return multiple objects in the array. \\\r\nIf no extractable fields are found, return an empty JSON array []. \\\r\nDo not provide explanations, summaries, or text outside of the JSON output.\" If the user provied text doesn't have any of the specified output json fields then return null but do not add the extra field yourself"
             },

             new Message { Role = MessageRoleType.User, Content = prompt.ToString() }
         );

            return response;
        }

    }

}
