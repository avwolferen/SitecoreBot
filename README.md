# Leveraging the Microsoft Bot Framework to operate Sitecore

This Bot was created for the SUGCON (Sitecore User Group Conference) held on May 18th and 19th 2017 in Amsterdam.

If you would like to read more about this Bot specifically you can take a look at my blog posts at https://suneco.nl/blogs/leveraging-the-microsoft-bot-framework-to-operate-sitecore

## Deploying this Bot
There are two project you should be aware of. First the Sitecore part of this solution which can be found in the project SugCon.Website. You need to publish this to your Sitecore instance. You only need this on your CM instance.

Next the project SugCon.Bot needs to be published to an AppService in Azure. This is the Bot itself.

You need to change a few things in configuration files to make it all work. The files you need to update are:
* /src/SugCon.Website/App_Config/Include/Bot/SugConBot.config
* /src/SugCon.Bot/Web.config
* /src/SugCon.Bot/ApplicationInsights.config

## Sitecore version
The Bot was built to work with Sitecore 8.2 update 3. For the Sitecore references I used the NuGet stream by Sitecore.

## SUGCON 2017
More about the presentation I gave together with Robbert Hock can be found on https://www.kayee.nl/2017/07/03/sugcon-europe-2017-event-bot-video-recording-and-code/

## About me
My name is Alex van Wolferen and I am a software developer at Suneco. If you need support or help you can contact me via Twitter on https://twitter.com/avwolferen