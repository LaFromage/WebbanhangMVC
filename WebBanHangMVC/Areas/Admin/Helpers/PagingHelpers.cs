using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList.Mvc.Core;
using X.PagedList;
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

public static class PagingHelper
{
    public static IHtmlContent PagedListPager<T>
    (this IHtmlHelper html, IPagedList<T>
        pagedList, Func<int, string>
            generatePageUrl, string cssClass = "pagination")
    {
        if (pagedList.PageCount <= 1)
        {
            return HtmlString.Empty;
        }

        var ulTag = new TagBuilder("ul");
        ulTag.AddCssClass(cssClass);

        // Previous Page
        var prevLiTag = new TagBuilder("li");
        prevLiTag.AddCssClass("page-item");
        if (!pagedList.HasPreviousPage)
        {
            prevLiTag.AddCssClass("disabled");
        }

        var prevATag = new TagBuilder("a");
        prevATag.AddCssClass("page-link");
        prevATag.Attributes["href"] = pagedList.HasPreviousPage ? generatePageUrl(pagedList.PageNumber - 1) : "#";
        prevATag.InnerHtml.AppendHtml("&laquo;");
        prevLiTag.InnerHtml.AppendHtml(prevATag);
        ulTag.InnerHtml.AppendHtml(prevLiTag);

        // Page Numbers
        for (var i = 1; i <= pagedList.PageCount; i++)
        {
            var liTag = new TagBuilder("li");
            liTag.AddCssClass("page-item");
            if (i == pagedList.PageNumber)
            {
                liTag.AddCssClass("active");
            }

            var aTag = new TagBuilder("a");
            aTag.AddCssClass("page-link");
            aTag.Attributes["href"] = generatePageUrl(i);
            aTag.InnerHtml.AppendHtml(i.ToString());
            liTag.InnerHtml.AppendHtml(aTag);
            ulTag.InnerHtml.AppendHtml(liTag);
        }

        // Next Page
        var nextLiTag = new TagBuilder("li");
        nextLiTag.AddCssClass("page-item");
        if (!pagedList.HasNextPage)
        {
            nextLiTag.AddCssClass("disabled");
        }

        var nextATag = new TagBuilder("a");
        nextATag.AddCssClass("page-link");
        nextATag.Attributes["href"] = pagedList.HasNextPage ? generatePageUrl(pagedList.PageNumber + 1) : "#";
        nextATag.InnerHtml.AppendHtml("&raquo;");
        nextLiTag.InnerHtml.AppendHtml(nextATag);
        ulTag.InnerHtml.AppendHtml(nextLiTag);

        return ulTag;
    }
}