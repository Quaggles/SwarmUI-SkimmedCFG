postParamBuildSteps.push(() => {
    let group = document.getElementById('input_group_content_skimmedcfg');
    if (group) {
        if (!currentBackendFeatureSet.includes('skimmedcfg')) {
            group.append(createDiv(`reactor_skimmedcfg_install_button`, 'keep_group_visible', `<button class="basic-button" onclick="installFeatureById('skimmedcfg', 'reactor_skimmedcfg_install_button')">Install SkimmedCFG</button>`));
        }
    }
});
