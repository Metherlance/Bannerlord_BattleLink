:: launch in administrator
:: language sensitve...
netsh http add urlacl url=http://+:7211/battlelink/api/battles/ user=everyone
::netsh http add urlacl url=http://+:7211/battlelink/api/battles/ user="Tout le monde"
::netsh http delete urlacl url=http://+:7211/battlelink/api/battles/