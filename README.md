# Overview
Asp.Net Core 3.1 web api site providing GET endpoint /color/{encodedImageUri} to return a color catalogue named color.  All spellings in this solution have been written as US English color to avoid mixing and matching with British English and the confusion that causes.
Services and their logic are implemented in the ColorMatcher.Logic class library with unit tests implemented in the ColorMatcher.Logic.Test project.
The pixel at postion (0,0) (configurable via appsettings ImageOptions section) is used to color match. An exact match is attempted in the color catalogue using an O(1) lookup with a fallback to an attempt to find a color sufficiently close in RGB values based on appsetting RgbFuzziness.  An RgbFuzzines of value of 2 means a catalogue color with all rgb values being within 2 from the color to match would be deemed acceptable.


The fuzzy search attempts to discard large parts of the color catalogue quickly during fuzzy searching rather than iterating the whole catalogue to perform rgb variance calculations. This was done in anticipation of a real life system supporting a large number of colors and the need for color matching to be efficient.


Response are simple text as requested. For real usage well formed and defined Json responses should be returned.


# Scaling at the infrustructure level
The API was been written as an Asp .Net Core Web API in anticipation of this being the most likely existing technology in use by Pwinty.
Docker imaging was not implemented but a deployment process involving Docker Imaging and orchestrated by Kubernetes would provide the ability to scale pod replicas on demand.


Alternatively if a serverless technology such as Azure Functions or AWS Lambas are in use already by Pwinty then the application could be converted to a serverless function triggered by Http requests which can be elastically scaled with demand.


# Scaling at the code level
The code has been written to utilise async for IO to support higher scalability.


Some online resources suggested using System.Drawing from new .Net standard/core code was not a good idea as it was there for ease of transtion and compatibility but may still suffer from historical performance issues
<https://www.hanselman.com/blog/HowDoYouUseSystemDrawingInNETCore.aspx>


The first alternative library I looked at was ImageSharp which uses the Affero GPL. There were conflicting reports online as to whether the AGPL meant the application's source code had to released and therefore it may be an unsuitable commercial license. I decided for the purpose of this exercise to continue with the System.Drawing.Common package provided from Microsoft to avoid license encumberment. This may be a point of both scalability and performance improvements by using an alternative library. Any switch of library should be evidence based with direct performance comparisons conducted.


If high volume customers repeatedly call to color match with the same image urls then HTTP Response caching could be implemented to avoid the need for the server to repeatedly execute the web api actions for the same image urls.


# Efficiency
If a customer already uploads their image to Prodigi during the rest of an order workflow outwith color matching, that would provide the potential hook to execute the color match rather than a separate API call which performs an expensive download of a large image purely to check one pixel.
Another alternative is to provide an SDK/API client for use with Pwinty that at some point during the workflow can get access to a relevant pixel to obtain it's RGB values to then make a more optimised call to an endpoint that takes purely the RBG values and returns the catalogue color. Thie SDK/client could also implement client side response caching if repeatedly using the same image for color matching.
An endpoint to provide all the color catalogue to pre-empt the issue could also help where a customer could allow users to only pick from supported colors in the first instance.


# Resilience
With the Kubernetes orchestrated hosting if a pod goes down kubernetes will automatically spint up new pods maintaining the desired numbers. Similarly servless functions can spin up new instances as required. Hosting in a cloud provider should follow high availability best practises and have instances of the API in more than one geographical region's data centre with data storage geo replicated between geographical regions.  In Azure this typically means having a Traffic Manager / API management component on top to route the traffic to secondary if the primary goes down. A circuit breaker pattern implementation would likely be required to ensure the circuit is tripped to switch to secondary if primary is generating errors.


# Alerting/monitoring
Logging has been implemented of exceptions during color matching.  This could be configured for ApplicationInsights/database/file logging. Proactive monitoring of these logs is encoruaged. Additionally .Net Core 3 diagnostic tools for event counters could be used by adding relevant metric points to the code. Metrics for average time to process a request, total number of requests in the last 15 minutes (configurable interval) would be useful for identifying bottlenecks and potentially stoppages combined with an API heartbeat.  .net core also has tracing and crash dump analysis tooling.  See <https://devblogs.microsoft.com/dotnet/introducing-diagnostics-improvements-in-net-core-3-0/>.