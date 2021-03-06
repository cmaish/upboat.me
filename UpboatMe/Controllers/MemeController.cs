﻿using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using UpboatMe.Imaging;
using UpboatMe.Models;
using UpboatMe.Utilities;

namespace UpboatMe.Controllers
{
    public class MemeController : Controller
    {
        private static Regex _StripRegex = new Regex(@"&?drawboxes=true|\.png$|\.jpe?g$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static Regex _UrlExtension = new Regex(@"\.png$|\.jpe?g$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ignore the parameters and figure it out manually from Request.RawUrl
        // this allows us to keep using routes to generate our own links, which is handy
        public ActionResult Make(string name)
        {
            var url = Request.ServerVariables["HTTP_URL"];
            
            // decode any %-encodings
            url = Server.UrlDecode(url);

            var hasExtension = _UrlExtension.IsMatch(url);

            // todo - handle this more elegantly, or don't do such things via this action
            var drawBoxes = url.Contains("drawBoxes=true");

            // strip off any file extensions
            url = _StripRegex.Replace(url, "");

            var memeRequest = MemeUtilities.GetMemeRequest(url);

            var meme = MemeUtilities.FindMeme(GlobalMemeConfiguration.Memes, memeRequest.Name);
            if (meme == null)
            {
                // TODO: update this flow to return a proper HTTP 404 code, too
                meme = GlobalMemeConfiguration.NotFoundMeme;
                memeRequest.Top = "404";
                memeRequest.Bottom = "Y U NO USE VALID MEME NAME?";
            }

            // if we want to do this earlier in the process consider
            // if we still need to worry about png vs. jpg. The browser
            // probably doesn't care, but it might be weird if the extension
            // doesn't match the mimetype
            if (!hasExtension)
            {
                return Redirect(url + Path.GetExtension(meme.ImageFileName));
            }

            var renderParameters = new RenderParameters
            {
                FullImagePath = HttpContext.Server.MapPath(meme.ImagePath),
                TopLineHeightPercent = meme.TopLineHeightPercent,
                BottomLineHeightPercent = meme.BottomLineHeightPercent,
                Fill = meme.Fill,
                Stroke = meme.Stroke,
                Font = meme.Font,
                FontSize = meme.FontSize,
                StrokeWidth = meme.StrokeWidth,
                DrawBoxes = drawBoxes,
                FullWatermarkImageFilePath = HttpContext.Server.MapPath("~/Content/UpBoatWatermark.png"),
                WatermarkImageHeight = 25,
                WatermarkImageWidth = 25,
                WatermarkText = "upboat.me",
                WatermarkFont = "Arial",
                WatermarkFontSize = 9,
                WatermarkStroke = Color.Black,
                WatermarkFill = Color.White,
                WatermarkStrokeWidth = 1
            };

            var renderer = new Renderer();

            var bytes = renderer.Render(renderParameters, memeRequest.Top.SanitizeMemeText(), memeRequest.Bottom.SanitizeMemeText());

            Analytics.TrackMeme(HttpContext, memeRequest.Name);

            return new FileContentResult(bytes, meme.ImageType);
        }

        public ActionResult Debug(string top, string bottom)
        {
            var viewModel = new MemeDebugViewModel();

            viewModel.DebugImages = GlobalMemeConfiguration.Memes.GetMemeNames().Select(m => string.Format("/{0}/{1}/{2}", m, top, bottom)).ToList();

            return View(viewModel);
        }

        public ActionResult List(string query)
        {
            var list = GlobalMemeConfiguration.Memes.GetMemes();

            if (!string.IsNullOrEmpty(query))
            {
                list = list.Where(m => m.Description.IndexOf(query) != -1 || m.Aliases.Any(a => a.IndexOf(query) != -1)).ToList();
            }

            return View(list);
        }

        public ActionResult Builder(string name, string top = "", string bottom = "")
        {
            var meme = MemeUtilities.FindMeme(GlobalMemeConfiguration.Memes, name);

            var viewModel = new BuilderViewModel
            {
                Memes = GlobalMemeConfiguration.Memes.GetMemes(),
                SelectedMeme = meme != null ? meme.Aliases.First() : string.Empty,
                Top = top,
                Bottom = bottom
            };

            return View(viewModel);
        }
    }
}
