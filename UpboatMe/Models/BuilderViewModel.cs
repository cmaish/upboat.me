﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using UpboatMe.Utilities;

namespace UpboatMe.Models
{
    public class BuilderViewModel
    {
        public string SelectedMeme { get; set; }
        public string Top { get; set; }
        public string Bottom { get; set; }
        public IList<Meme> Memes { get; set; }

        public string GetPreviewUrl(UrlHelper helper)
        {
            var path = helper.Action("Make", "Meme", new
                                                         {
                                                             name = SelectedMeme,
                                                             top = Top ?? "", // passing null breaks the route
                                                             bottom = Bottom ?? "" // passing null breaks the route
                                                         });
            return helper.AbsoluteAction(path);
        }

        public string GetAltText()
        {
            return string.Format("{0}:{1}{2}{1}{3}",
                                 SelectedMeme, Environment.NewLine, Top, Bottom);
        }

        public string GetShareText()
        {
            return string.Format("{0}", SelectedMeme);
        }

        public BuilderViewModel()
        {
            SelectedMeme = "ihyk";
            Top = "I'll-have-you-know-I-tried-other-meme-generators";
            Bottom = "and-only-wasted-hours-and-hours-of-my-life";
        }
    }
}