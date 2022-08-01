using AudibleApi.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    public class Item
    {
        public string ProductId => Asin;

        public bool IsInPlusCatalog => BenefitId == "AYCL" || IsAyce is true;

        public bool IsNotInPlusCatalog => !IsInPlusCatalog;

        public int LengthInMinutes => RuntimeLengthMin.GetValueOrDefault();

        public string Description => PublisherSummary;

        public bool IsEpisodes => Relationships?.Any((Relationship r) => (r.RelationshipToProduct == "child" || r.RelationshipToProduct == "parent") && r.RelationshipType == "episode") ?? false;

        public string PictureId => ProductImages?.PictureId;

        public DateTime DateAdded => PurchaseDate.UtcDateTime;

        public float Product_OverallStars => Convert.ToSingle((Rating?.OverallDistribution?.DisplayStars).GetValueOrDefault());

        public float Product_PerformanceStars => Convert.ToSingle((Rating?.PerformanceDistribution?.DisplayStars).GetValueOrDefault());

        public float Product_StoryStars => Convert.ToSingle((Rating?.StoryDistribution?.DisplayStars).GetValueOrDefault());

        public int MyUserRating_Overall => Convert.ToInt32((ProvidedReview?.Ratings?.OverallRating).GetValueOrDefault());

        public int MyUserRating_Performance => Convert.ToInt32((ProvidedReview?.Ratings?.PerformanceRating).GetValueOrDefault());

        public int MyUserRating_Story => Convert.ToInt32((ProvidedReview?.Ratings?.StoryRating).GetValueOrDefault());

        public bool IsAbridged => FormatType == "abridged";

        public string Abrigment => FormatType == "abridged" ? "Abridged" : "Unabridged";

        public DateTime? DatePublished => IssueDate?.UtcDateTime;

        public string Publisher => PublisherName;

        public Ladder[] Categories => CategoryLadders?.FirstOrDefault()?.Ladder ?? new Ladder[0];

        public Ladder ParentCategory => Categories?.FirstOrDefault();

        public Ladder ChildCategory
        {
            get
            {
                if (Categories.Length <= 1)
                {
                    return null;
                }

                return Categories[1];
            }
        }

        public string TitleWithSubtitle
        {
            get
            {
                string text = Title?.Trim();
                string text2 = Subtitle?.Trim();
                if (string.IsNullOrWhiteSpace(text2))
                {
                    return text;
                }

                return text + ": " + text2;
            }
        }

        [JsonProperty("asin")]
        public string Asin
        {
            get;
            set;
        }

        [JsonProperty("audible_editors_summary")]
        public string AudibleEditorsSummary
        {
            get;
            set;
        }

        [JsonProperty("authors")]
        public Person[] Authors
        {
            get;
            set;
        }

        [JsonProperty("availability")]
        public object Availability
        {
            get;
            set;
        }

        [JsonProperty("available_codecs")]
        public AvailableCodec[] AvailableCodecs
        {
            get;
            set;
        }

        [JsonProperty("badge_types")]
        public object BadgeTypes
        {
            get;
            set;
        }

        [JsonProperty("benefit_id")]
        public string BenefitId
        {
            get;
            set;
        }

        [JsonProperty("buying_options")]
        public object BuyingOptions
        {
            get;
            set;
        }

        [JsonProperty("category_ladders")]
        public CategoryLadder[] CategoryLadders
        {
            get;
            set;
        }

        [JsonProperty("claim_code_url")]
        public object ClaimCodeUrl
        {
            get;
            set;
        }

        [JsonProperty("collection_ids")]
        public object CollectionIds
        {
            get;
            set;
        }

        [JsonProperty("content_delivery_type")]
        public string ContentDeliveryType
        {
            get;
            set;
        }

        [JsonProperty("content_level")]
        public object ContentLevel
        {
            get;
            set;
        }

        [JsonProperty("content_rating")]
        public ContentRating ContentRating
        {
            get;
            set;
        }

        [JsonProperty("content_type")]
        public string ContentType
        {
            get;
            set;
        }

        [JsonProperty("copyright")]
        public object Copyright
        {
            get;
            set;
        }

        [JsonProperty("credits_required")]
        public object CreditsRequired
        {
            get;
            set;
        }

        [JsonProperty("customer_reviews")]
        public Review[] CustomerReviews
        {
            get;
            set;
        }

        [JsonProperty("date_first_available")]
        public object DateFirstAvailable
        {
            get;
            set;
        }

        [JsonProperty("distribution_rights_region")]
        public object DistributionRightsRegion
        {
            get;
            set;
        }

        [JsonProperty("editorial_reviews")]
        public string[] EditorialReviews
        {
            get;
            set;
        }

        [JsonProperty("extended_product_description")]
        public object ExtendedProductDescription
        {
            get;
            set;
        }

        [JsonProperty("format_type")]
        public string FormatType
        {
            get;
            set;
        }

        [JsonProperty("generic_keyword")]
        public object GenericKeyword
        {
            get;
            set;
        }

        [JsonProperty("has_children")]
        public bool? HasChildren
        {
            get;
            set;
        }

        [JsonProperty("image_url")]
        public object ImageUrl
        {
            get;
            set;
        }

        [JsonProperty("invites_remaining")]
        public object InvitesRemaining
        {
            get;
            set;
        }

        [JsonProperty("is_adult_product")]
        public bool? IsAdultProduct
        {
            get;
            set;
        }

        [JsonProperty("is_ayce")]
        public bool? IsAyce
        {
            get;
            set;
        }

        [JsonProperty("is_buyable")]
        public object IsBuyable
        {
            get;
            set;
        }

        [JsonProperty("is_downloaded")]
        public bool? IsDownloaded
        {
            get;
            set;
        }

        public bool ActualIsDownloaded => GetIsDownloaded();

        private bool GetIsDownloaded()
        {
            if (!Directory.Exists(Constants.DownloadFolder))
                return false;
            string[] files = Directory.GetFiles(Constants.DownloadFolder, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                if (!name.Contains("PART"))
                {
                    string n = Path.GetFileNameWithoutExtension(file);
                    if (n == this.Asin)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [JsonProperty("is_finished")]
        public object IsFinished
        {
            get;
            set;
        }

        [JsonProperty("is_in_wishlist")]
        public object IsInWishlist
        {
            get;
            set;
        }

        public bool IsNotInWishlist => IsInWishlist is bool && IsInWishlist is false;

        [JsonProperty("is_listenable")]
        public bool? IsListenable
        {
            get;
            set;
        }

        [JsonProperty("is_pdf_url_available")]
        public object IsPdfUrlAvailable
        {
            get;
            set;
        }

        [JsonProperty("is_playable")]
        public object IsPlayable
        {
            get;
            set;
        }

        [JsonProperty("is_preorderable")]
        public object IsPreorderable
        {
            get;
            set;
        }

        [JsonProperty("is_returnable")]
        public bool? IsReturnable
        {
            get;
            set;
        }

        [JsonProperty("is_searchable")]
        public object IsSearchable
        {
            get;
            set;
        }

        [JsonProperty("is_visible")]
        public object IsVisible
        {
            get;
            set;
        }

        [JsonProperty("is_ws4v_companion_asin_owned")]
        public object IsWs4VCompanionAsinOwned
        {
            get;
            set;
        }

        [JsonProperty("is_ws4v_enabled")]
        public object IsWs4VEnabled
        {
            get;
            set;
        }

        [JsonProperty("isbn")]
        public object Isbn
        {
            get;
            set;
        }

        [JsonProperty("issue_date")]
        public DateTimeOffset? IssueDate
        {
            get;
            set;
        }

        [JsonProperty("language")]
        public string Language
        {
            get;
            set;
        }

        [JsonProperty("library_status")]
        public LibraryStatus LibraryStatus
        {
            get;
            set;
        }

        [JsonProperty("member_giving_status")]
        public object MemberGivingStatus
        {
            get;
            set;
        }

        [JsonProperty("merchandising_description")]
        public object MerchandisingDescription
        {
            get;
            set;
        }

        [JsonProperty("merchandising_summary")]
        public string MerchandisingSummary
        {
            get;
            set;
        }

        [JsonProperty("narration_accent")]
        public object NarrationAccent
        {
            get;
            set;
        }

        [JsonProperty("narrators")]
        public Person[] Narrators
        {
            get;
            set;
        }

        [JsonProperty("new_episode_added_date")]
        public object NewEpisodeAddedDate
        {
            get;
            set;
        }

        [JsonProperty("order_id")]
        public object OrderId
        {
            get;
            set;
        }

        [JsonProperty("order_item_id")]
        public object OrderItemId
        {
            get;
            set;
        }

        [JsonProperty("origin_asin")]
        public string OriginAsin
        {
            get;
            set;
        }

        [JsonProperty("origin_id")]
        public string OriginId
        {
            get;
            set;
        }

        [JsonProperty("origin_marketplace")]
        public string OriginMarketplace
        {
            get;
            set;
        }

        [JsonProperty("origin_type")]
        public string OriginType
        {
            get;
            set;
        }

        [JsonProperty("part_number")]
        public object PartNumber
        {
            get;
            set;
        }

        [JsonProperty("pdf_url")]
        public Uri PdfUrl
        {
            get;
            set;
        }

        [JsonProperty("percent_complete")]
        public double? PercentComplete
        {
            get;
            set;
        }

        [JsonProperty("periodical_info")]
        public object PeriodicalInfo
        {
            get;
            set;
        }

        [JsonProperty("plans")]
        public Plan[] Plans
        {
            get;
            set;
        }

        [JsonProperty("platinum_keywords")]
        public object PlatinumKeywords
        {
            get;
            set;
        }

        [JsonProperty("preorder_release_date")]
        public object PreorderReleaseDate
        {
            get;
            set;
        }

        [JsonProperty("preorder_status")]
        public object PreorderStatus
        {
            get;
            set;
        }
        [JsonProperty("price")]
        public Price Price
        {
            get;
            set;
        }
        [JsonProperty("product_images")]
        public ProductImages ProductImages
        {
            get;
            set;
        }
        [JsonProperty("product_page_url")]
        public object ProductPageUrl
        {
            get;
            set;
        }
        [JsonProperty("product_site_launch_date")]
        public object ProductSiteLaunchDate
        {
            get;
            set;
        }
        [JsonProperty("provided_review")]
        public Review ProvidedReview
        {
            get;
            set;
        }
        [JsonProperty("publication_name")]
        public string PublicationName
        {
            get;
            set;
        }
        [JsonProperty("publisher_name")]
        public string PublisherName
        {
            get;
            set;
        }
        [JsonProperty("publisher_summary")]
        public string PublisherSummary
        {
            get;
            set;
        }
        [JsonProperty("purchase_date")]
        public DateTimeOffset PurchaseDate
        {
            get;
            set;
        }
        [JsonProperty("rating")]
        public Rating Rating
        {
            get;
            set;
        }
        [JsonProperty("read_along_support")]
        public object ReadAlongSupport
        {
            get;
            set;
        }
        [JsonProperty("relationships")]
        public Relationship[] Relationships
        {
            get;
            set;
        }
        [JsonProperty("release_date")]
        public DateTimeOffset? ReleaseDate
        {
            get;
            set;
        }
        [JsonProperty("review_status")]
        public object ReviewStatus
        {
            get;
            set;
        }
        [JsonProperty("runtime_length_min")]
        public int? RuntimeLengthMin
        {
            get;
            set;
        }
        [JsonProperty("sample_url")]
        public Uri SampleUrl
        {
            get;
            set;
        }
        [JsonProperty("series")]
        public Series[] Series
        {
            get;
            set;
        }
        [JsonProperty("sku")]
        public string Sku
        {
            get;
            set;
        }
        [JsonProperty("sku_lite")]
        public string SkuLite
        {
            get;
            set;
        }
        [JsonProperty("status")]
        public string Status
        {
            get;
            set;
        }
        [JsonProperty("subscription_asins")]
        public object SubscriptionAsins
        {
            get;
            set;
        }
        [JsonProperty("subtitle")]
        public string Subtitle
        {
            get;
            set;
        }
        [JsonProperty("thesaurus_subject_keywords")]
        public string[] ThesaurusSubjectKeywords
        {
            get;
            set;
        }
        [JsonProperty("title")]
        public string Title
        {
            get;
            set;
        }
        [JsonProperty("voice_description")]
        public object VoiceDescription
        {
            get;
            set;
        }
        [JsonProperty("ws4v_companion_asin")]
        public object Ws4VCompanionAsin
        {
            get;
            set;
        }
        public bool IsInLibrary => PurchaseDate != DateTimeOffset.MinValue;

        public bool IsNotInLibrary => !IsInLibrary;



        public override string ToString()
        {
            return "[" + ProductId + "] " + Title;
        }
    }
}
