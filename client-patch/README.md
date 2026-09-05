# Pasta de delta para o workflow client-patch.yml
#
# Espelhe o caminho relativo ao client. Exemplos:
#   client-patch/Config/foo.ini
#   client-patch/FrontLine.exe
#   client-patch/Gui/Something.i3i
#
# NAO coloque aqui:
#   - filelist-private.pem (vai no secret FILELIST_PRIVATE_PEM)
#   - Shop.dat / EventPortal.dat (sincronizados pelo jogo; fora do FL Guard)
#   - packs enormes sem necessidade (prefira patch minimo)
#
# Depois: git tag client-vYYYYMMDD && git push origin client-vYYYYMMDD
#
# Ver docs/deploy-github-actions.md
