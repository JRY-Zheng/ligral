import sys
import markdown
from bs4 import BeautifulSoup

with open('README.md', 'r', encoding='utf8') as f:
    text = f.read()

html_text = markdown.markdown(text, extensions=['markdown.extensions.tables'])
html = BeautifulSoup(html_text)

with open('web/index.html', 'r', encoding='utf8') as f:
    text = f.read()

soup = BeautifulSoup(text)
articles = soup.find_all('article')
if len(articles) == 0:
    print('ERROR: no article was found.')
    sys.exit(1)
elif len(articles) > 1:
    print('WARN: multiple articles were found, use the first.')
else:
    article = articles[0]
    bodies = html.find_all('body')
    body = bodies[0]
    for i, child in enumerate(body.children):
        article.insert(i, child)

with open('web/index.html', 'w', encoding='utf8') as f:
    f.write(soup.prettify())