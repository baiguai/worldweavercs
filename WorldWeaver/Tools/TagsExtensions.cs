namespace WorldWeaver.Tools
{
    public static class TagsExtensions
    {
        public static bool TagsContain(this string tags, string searchString)
        {
            List<string> tgs = SplitTags(tags);

            return tgs.Contains(searchString);
        }

        public static bool ListContains(this string listItems, string searchString)
        {
            List<string> lst = SplitTags(listItems);

            return lst.Contains(searchString);
        }

        public static string AddTag(this string tags, string newTag)
        {
            List<string> tgs = SplitTags(tags);
            if (tgs.Count == 0 || (tgs.Count == 1 && tgs[0].Equals("")))
            {
                return newTag;
            }
            tgs.Add(newTag);

            tags = JoinTags(tgs);
            return tags;
        }

        public static string RemoveTag(this string tags, string tagToRemove)
        {
            List<string> tgs = SplitTags(tags);
            if (tgs.Count == 0)
            {
                return "";
            }
            tgs.Remove(tagToRemove);

            return JoinTags(tgs);
        }

        public static string ReplaceTag(this string tags, string tagToReplace, string newTag)
        {
            List<string> tgs = SplitTags(tags);
            tgs.Remove(tagToReplace);
            tgs.Add(newTag);

            return JoinTags(tgs);
        }



        private static List<string> SplitTags(string tags)
        {
            return tags.Split('|').ToList();
        }

        private static string JoinTags(List<string> taglist)
        {
            taglist.Remove("");
            if (taglist.Count == 1)
            {
                return taglist[0];
            }
            else
            {
                return string.Join("|", taglist);
            }
        }
    }
}
