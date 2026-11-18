export const TalkwalkerPostListTemplate = `@using System
@using System.Collections.Generic
@using System.Dynamic
@using System.Linq
@using TNO.TemplateEngine

@{
  var items = Data as IEnumerable<dynamic>;

  if (items == null || !items.Any())
  {
      <p>No results available.</p>
      return;
  }

  @functions {
    public string? FormatMetric(IDictionary<string, object> dict, string key, string label)
    {
      if (dict.ContainsKey(key)
        && !String.IsNullOrWhiteSpace(@dict[key] as string)
        && int.TryParse(@dict[key] as string, out int value)
        && value > 0)
      {
        return $"<span style=\\"font-weight: 600;\\">" + ReportExtensions.FormatInteger(dict[key] as String, true) + "</span> X Shares,";
      }
      return null;
    }
  }
}

@if (items != null)
{
  <table style="border-collapse: collapse; width: 100%; margin: 0 auto; font-family: Arial, Helvetica, sans-serif; font-size: 14px; color: #333333;">
    @foreach (var item in items)
    {
      var dict = item as IDictionary<string, object>;
      if (dict != null)
      {
        var sentimentStr = dict.ContainsKey("sentiment") ? dict["sentiment"] as String : null;
        int? sentiment = sentimentStr != null && int.TryParse(sentimentStr.Replace("'",""), out int sentimentValue) ? sentimentValue : null;
        var engagement = dict.ContainsKey("engagement") ? ReportExtensions.FormatInteger(dict["engagement"] as String, true) : null;
        var reach = dict.ContainsKey("reach") ? ReportExtensions.FormatInteger(dict["reach"] as String, true) : null;
        var author_image_url = dict.ContainsKey("extra_author_attributes.image_url") ? dict["extra_author_attributes.image_url"] as String : null;
        var title = dict.ContainsKey("title") ? dict["title"] as String : dict.ContainsKey("title_snippet") ? dict["title_snippet"] as String : null;
        var author_name = dict.ContainsKey("extra_author_attributes.name") ? dict["extra_author_attributes.name"] as String : null;
        var author_short_name = dict.ContainsKey("extra_author_attributes.short_name") ? dict["extra_author_attributes.short_name"] as String : null;
        var post_type = dict.ContainsKey("post_type") ? dict["post_type"] as String : null;
        var url = dict.ContainsKey("url") ? dict["url"] as String : null;
        var host_url = dict.ContainsKey("host_url") ? dict["host_url"] as String : null;
        var root_url = dict.ContainsKey("root_url") ? dict["root_url"] as String : null;
        var images_url = dict.ContainsKey("images.url") ? dict["images.url"] as String : null;
        var content = dict.ContainsKey("content") ? dict["content"] as String : null;
        var published = dict.ContainsKey("published") ? dict["published"] as String : null;
        var country = dict.ContainsKey("extra_source_attributes.world_data.country") ? dict["extra_source_attributes.world_data.country"] as String : null;
        var source_name = dict.ContainsKey("extra_source_attributes.name") ? dict["extra_source_attributes.name"] as String : null;

        <tr style="height: 100%; width: 100%; box-sizing: border-box;">
          <td style="padding: 1rem; border-bottom: 1px solid #dddddd; width: 80px; height: 100%; box-sizing: border-box;">
            @if (!string.IsNullOrWhiteSpace(author_image_url))
            {
              <div style="width: 50px; height: 50px; border-radius: 50%; overflow: hidden; display: inline-block; text-align: center;">
                <a href="@root_url"><img src="@author_image_url" style="width: 100%; height:100%; object-fit: cover; display: block;"/></a>
              </div>
            }
          </td>
          <td style="padding: 1rem; border-bottom: 1px solid #dddddd; max-width: 65%; height: 100%; box-sizing: border-box;">
            <div style="width: 100%; height: 100%; box-sizing: border-box;">
              <div>
                @if (!string.IsNullOrWhiteSpace(title))
                {
                  <span style="font-weight: 600; margin-right: 0.5rem;"><a href="@dict["url"]" style="color: rgb(52, 81, 153);">@title</a></span>
                }
                <span style="font-weight: 600; margin-right: 1rem; text-transform: capitalize;"><a href="@root_url" style="color: rgb(52, 81, 153);">@author_name</a></span>
                @if (!string.IsNullOrWhiteSpace(author_short_name))
                {
                  <span style="margin-right: 0.5rem;"><a href="@root_url" style="color: rgb(52, 81, 153);">@@@author_short_name</a></span>
                }
                <span style="text-transform: lowercase; font-size: 0.85rem;">
                  @if (post_type == "IMAGE")
                  {
                    @("shared an image")
                  }
                  else if (post_type == "LINK")
                  {
                    @("shared a link")
                  }
                  else if (post_type == "TEXT")
                  {
                    @("shared a post")
                  }
                  else
                  {
                    @("shared a " + post_type)
                  }
                </span>
              </div>
              <div style="padding-top: 1rem;">
                <table style="border-collapse: collapse;">
                  <tr>
                    @if (!string.IsNullOrWhiteSpace(images_url))
                    {
                      <td style="padding: 1rem;"><a href="@url"><img src="@images_url" style="max-width: 300px;" /></a></td>
                    }
                    <td><p>@content</p></td>
                  </tr>
                </table>
              </div>
              <div style="font-size: 0.85rem; padding-top: 1rem;">
                <a href="@url" style="color: rgb(52, 81, 153);">published on @published</a> | @country | <a href="@host_url">@source_name</a>
              </div>
            </div>
          </td>
          <td style="padding: 1rem; border-bottom: 1px solid #dddddd; width: 35%; height: 100%; box-sizing: border-box;">
            <div style="padding: 1rem; margin: 1rem; border-left: 1px solid #dddddd;">
              <table style="border-collapse: collapse;">
                <tr>
                  <td style="padding-bottom: 1rem; text-align: center;">
                    <div>@ReportExtensions.GetSentimentIcon(sentiment)</div>
                  </td>
                </tr>
                <tr>
                  <td style="font-weight: 600; color: rgb(52, 81, 153); text-align: center; vertical-align: top;">METRICS</td>
                  <td style="text-align: left; vertical-align: top; padding: 0 1rem;">
                    <div styles="">
                      <table style="border-collapse: collapse;">
                        <tr>
                          <td>
                            Engagement: <span style="font-weight: 600; color: rgb(52, 81, 153);">@engagement</span>
                          </td>
                          <td>
                            Reach: <span style="font-weight: 600; color: rgb(52, 81, 153);">@reach</span>
                          </td>
                        </tr>
                      </table>
                    </div>
                    <div style="font-size: 0.85rem; padding-top: 1rem;">
                      @FormatMetric(dict, "article_extended_attributes.twitter_shares", "X Shares")
                      @FormatMetric(dict, "article_extended_attributes.twitter_retweets", "X Reposts")
                      @FormatMetric(dict, "article_extended_attributes.twitter_quote_tweets", "X Quotes")
                      @FormatMetric(dict, "article_extended_attributes.twitter_replies", "X Replies")
                      @FormatMetric(dict, "article_extended_attributes.twitter_likes", "X Likes")
                      @FormatMetric(dict, "article_extended_attributes.twitter_followers", "X Followers")
                      @FormatMetric(dict, "article_extended_attributes.twitter_impressions", "X Impressions")
                      @FormatMetric(dict, "article_extended_attributes.twitter_video_views", "X Video Views")

                      @FormatMetric(dict, "article_extended_attributes.semrush_unique_visitors", "Semrush Traffic Rank")
                      @FormatMetric(dict, "article_extended_attributes.semrush_pageviews", "Semrush Page Views")

                      @FormatMetric(dict, "article_extended_attributes.alexa_unique_visitors", "Alexa Unique Visitors")
                      @FormatMetric(dict, "article_extended_attributes.alexa_pageviews", "Alexa Page Views")

                      @FormatMetric(dict, "article_extended_attributes.linkedin_shares", "LinkedIn Shares")
                      @FormatMetric(dict, "article_extended_attributes.linkedin_impression", "LinkedIn Impressions")

                      @FormatMetric(dict, "article_extended_attributes.facebook_shares", "Facebook Shares")
                      @FormatMetric(dict, "article_extended_attributes.facebook_reactions_total", "Facebook Reactions")
                      @FormatMetric(dict, "article_extended_attributes.facebook_likes", "Facebook Likes")
                      @FormatMetric(dict, "article_extended_attributes.facebook_reactions_haha", "Facebook HaHa")
                      @FormatMetric(dict, "article_extended_attributes.facebook_reactions_angry", "Facebook Angry")
                      @FormatMetric(dict, "article_extended_attributes.facebook_reactions_sad", "Facebook Sad")
                      @FormatMetric(dict, "article_extended_attributes.facebook_reactions_love", "Facebook Love")
                      @FormatMetric(dict, "article_extended_attributes.facebook_reactions_wow", "Facebook Wow")
                      @FormatMetric(dict, "article_extended_attributes.facebook_followers", "Facebook Followers")
                      @FormatMetric(dict, "article_extended_attributes.facebook_group_members", "Facebook Group Members")

                      @FormatMetric(dict, "article_extended_attributes.pinterest_likes", "Pinterest Likes")
                      @FormatMetric(dict, "article_extended_attributes.pinterest_pins", "Pinterest Pins")
                      @FormatMetric(dict, "article_extended_attributes.pinterest_repins", "Pinterest Repins")
                      @FormatMetric(dict, "article_extended_attributes.pinterest_followers", "Pinterest Followers")

                      @FormatMetric(dict, "article_extended_attributes.youtube_views", "YouTube Views")
                      @FormatMetric(dict, "article_extended_attributes.youtube_likes", "YouTube Likes")
                      @FormatMetric(dict, "article_extended_attributes.youtube_dislikes", "YouTube Dislikes")

                      @FormatMetric(dict, "article_extended_attributes.instagram_likes", "Instagram Likes")
                      @FormatMetric(dict, "article_extended_attributes.instagram_followers", "Instagram Followers")
                    </div>
                  </td>
                </tr>
              </table>
            </div>
          </td>
        </tr>
      }
      else
      {
        <div>Failed to parse data.</div>
      }
    }
  </table>
}
else
{
  <div>No data found.</div>
}
`;
