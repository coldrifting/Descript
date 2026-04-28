using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Descript.Models;

namespace Descript.Utils;

public static class Extensions {
    extension(string str)
    {
        public string ToTitleCase()
        {
            TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        
            return str.Length switch
            {
                0 => str,
                1 => str.ToUpper(),
                _ => textInfo.ToTitleCase(str)
            };
        }

        public bool ContainsTrimmed(string b)
        {
            return str.Trim().Contains(b.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
    
    extension(Enum value) 
    {
        public string GetDescription()
        {
            Type type = value.GetType();
            FieldInfo? fieldInfo = type.GetField(value.ToString());
            if (fieldInfo is null)
            {
                return value.ToString();
            }
            
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }

    extension(IEnumerable<Element> elements)
    {
        public IEnumerable<Element> WithSelection(int selection)
        {
            if (selection == 0)
            {
                return elements.Select(element => element.Id == selection ? element.WithSelected : element);
            }

            List<Element> elementsList = elements.ToList();
            return elementsList.Select(element => element.Glyph).Contains(Element.GlyphFromId(selection)) 
                ? elementsList.Select(element => element.Id == selection ? element.WithSelected : element) 
                : elementsList.Prepend(Element.FromIdSelected(selection));
        }
        
        public IOrderedEnumerable<Element> Ordered()
        {
            return elements
                .OrderByDescending(element => element.IsCurrentSelection)
                .ThenBy(element => element.Confidence)
                .ThenBy(element => element.Translation == "" ? "Ω" : element.Translation)
                .ThenBy(element => element.Glyph);
        }

        public IEnumerable<Element> Matching(string filterText, int selection)
        {
            return elements.Where(element => (element.Glyph & selection) == selection && Contains(element));

            bool Contains(Element element)
            {
                return filterText.Contains(element.Glyph) || element.Translation.Trim().Contains(filterText.Trim(), StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }
    
    extension(IEnumerable<Sentence> sentences)
    {
        public IOrderedEnumerable<Sentence> OrderBy(SentenceSortMode sortMode)
        {
            return sortMode switch
            {
                SentenceSortMode.ByCategory => sentences.OrderByCategory(),
                SentenceSortMode.ByLeastTranslated => sentences.OrderByLeastTranslated(),
                SentenceSortMode.ByMostTranslated => sentences.OrderByMostTranslated(),
                
                _ => throw new ArgumentOutOfRangeException(nameof(sortMode), sortMode, null)
            };
        }
        
        private IOrderedEnumerable<Sentence> OrderByCategory()
        {
            return sentences.OrderBy(sentence => sentence.Category)
                .ThenBy(sentence => sentence.SubCategory.ToLower())
                .ThenBy(sentence => sentence.Context.ToLower())
                .ThenBy(sentence => sentence.SentenceOriginal.ToLower());
        }

        private IOrderedEnumerable<Sentence> OrderByLeastTranslated()
        {
            return sentences
                .OrderByDescending(sentence => sentence.NumUntranslatedPhrases)
                .ThenByDescending(sentence => sentence.NumUntranslatedElements)
                .ThenBy(sentence => sentence.Category.ToLower())
                .ThenBy(sentence => sentence.SubCategory.ToLower())
                .ThenBy(sentence => sentence.Context.ToLower())
                .ThenBy(sentence => sentence.SentenceOriginal.ToLower());
        }
        
        private IOrderedEnumerable<Sentence> OrderByMostTranslated()
        {
            return sentences
                .OrderBy(sentence => sentence.NumUntranslatedPhrases)
                .ThenBy(sentence => sentence.NumUntranslatedElements)
                .ThenBy(sentence => sentence.Category.ToLower())
                .ThenBy(sentence => sentence.SubCategory.ToLower())
                .ThenBy(sentence => sentence.Context.ToLower())
                .ThenBy(sentence => sentence.SentenceOriginal.ToLower());
        }
    }
}