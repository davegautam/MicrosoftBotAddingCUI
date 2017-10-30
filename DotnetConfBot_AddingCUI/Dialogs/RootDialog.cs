using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Net.Http.Headers;

namespace DotnetConfBot_AddingCUI.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
           
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

            PromptDialog.Choice(
                   context: context,
                   resume: GetContentAsync,
                   options: Options,
                   prompt: "I can help you with Following",
                   retry: "Currently I dont Understand that. Please input what i understand");
        }
        public List<string> Options = new List<string>() {
           
            "View Sessions",
            "Verify Payment"
            
        };
        public async Task GetContentAsync(IDialogContext context, IAwaitable<string> result)
        {
            string selectedOption = await result;
            switch (selectedOption.ToString())
            {
                
                case "View Sessions":
                    var reply = context.MakeMessage();
                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments =await GetSessionDetail();
                    await context.PostAsync(reply);
                    await MessageReceivedAsync(context,result);
                    break;
                case "Verify Payment":
                    await context.PostAsync("File upload");
                  context.Wait(RecieveFileAsync);
                  
                    break;
                default:
                    break;
            }
           
        }

       

        private async Task<List<Attachment>> GetSessionDetail()
        {
            List<Attachment> attachmentList = new List<Attachment>();
            var alokIntroCard = new HeroCard()
            {
                Title = "C# Internals",
                Subtitle ="Alok Kumar Pandey",
                Text = "CTO @ Braindigit IT Colutions",
                Images = new List<CardImage> { new CardImage("http://dotnetconf.aspnetcommunity.org/Media/aloksir_2017_10_05_01_37_15.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "View Bio", value: "http://dotnetconf.aspnetcommunity.org/blog/Post/14/Net-Conf-2017/") }
            }; ;
            
            var devIntroCard = new HeroCard()
            {
                Title = "Intelligent Bots",
                Subtitle = "Dev Raj Gautam",
                Text = "Project Manager @ Braindigit IT Colutions",
                Images = new List<CardImage> { new CardImage("http://dotnetconf.aspnetcommunity.org/Media/devsir_2017_10_05_01_37_16.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "View Bio", value: "http://dotnetconf.aspnetcommunity.org/blog/Post/14/Net-Conf-2017/") }
            }; ;
            var ranjanIntroCard = new HeroCard()
            {
                Title = "React With .NET",
                Subtitle = "Ranjan Shrestha",
                Text = "CTO @ Bhoos Entertinment",
                Images = new List<CardImage> { new CardImage("http://dotnetconf.aspnetcommunity.org/Media/ranjan_2017_10_05_01_37_18.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "View Bio", value: "http://dotnetconf.aspnetcommunity.org/blog/Post/14/Net-Conf-2017/") }
            }; ;
            attachmentList.Add(alokIntroCard.ToAttachment());
            attachmentList.Add(devIntroCard.ToAttachment());

            attachmentList.Add(ranjanIntroCard.ToAttachment());
           return  attachmentList;
        }


        private async Task RecieveFileAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
           
            var message = await argument;
            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();
                using (HttpClient httpClient = new HttpClient())
                {
                    // Skype & MS Teams attachment URLs are secured by a JwtToken, so we need to pass the token from our bot.
                    if ((message.ChannelId.Equals("skype", StringComparison.InvariantCultureIgnoreCase) || message.ChannelId.Equals("msteams", StringComparison.InvariantCultureIgnoreCase))
                        && new Uri(attachment.ContentUrl).Host.EndsWith("skype.com"))
                    {
                        var token = await new MicrosoftAppCredentials().GetTokenAsync();
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    var responseMessage = await httpClient.GetAsync(attachment.ContentUrl);

                    var contentLenghtBytes = responseMessage.Content.Headers.ContentLength;
                    
                    await context.PostAsync($"Voucher Image of {attachment.ContentType} type and size of {contentLenghtBytes} bytes received. We will verify and Send an email for confirmation");
                    
                }
            }
            await MessageReceivedAsync(context, argument);

        }






    }
}