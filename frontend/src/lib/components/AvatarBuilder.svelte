<script lang="ts">
	import type { SpriteDto } from '$lib/api/sprites';

	type SelectionsMap = Record<string, string>;

	let {
		catalog,
		selections = $bindable()
	}: {
		catalog: SpriteDto[];
		selections: SelectionsMap;
	} = $props();

	const TYPE_DISPLAY_ORDER = [
		'BodyArchetype', 'Face', 'Eyes', 'Hair', 'OutfitTop', 'OutfitBottom', 'Accessory'
	];

	function groupedByType(catalog: SpriteDto[]) {
		const groups: Record<string, SpriteDto[]> = {};
		for (const sprite of catalog) {
			(groups[sprite.type] ??= []).push(sprite);
		}
		for (const type in groups) {
			groups[type].sort((a, b) => a.name.localeCompare(b.name));
		}
		return groups;
	}

	let groups = $derived(groupedByType(catalog));

	function cycle(type: string, direction: -1 | 1) {
		const typeSprites = groups[type];
		if (!typeSprites || typeSprites.length === 0) return;
		const currentId = selections[type];
		const currentIndex = typeSprites.findIndex((s) => s.id === currentId);
		const startIndex = currentIndex < 0 ? 0 : currentIndex;
		const nextIndex = (startIndex + direction + typeSprites.length) % typeSprites.length;
		selections = { ...selections, [type]: typeSprites[nextIndex].id };
	}

	function randomize() {
		const newSelections: SelectionsMap = {};
		for (const type of TYPE_DISPLAY_ORDER) {
			const typeSprites = groups[type];
			if (!typeSprites || typeSprites.length === 0) continue;
			const random = typeSprites[Math.floor(Math.random() * typeSprites.length)];
			newSelections[type] = random.id;
		}
		selections = newSelections;
	}

	function selectedInLayerOrder() {
		return Object.values(selections)
			.map((id) => catalog.find((s) => s.id === id))
			.filter((s): s is SpriteDto => s !== undefined)
			.sort((a, b) => a.layer - b.layer);
	}

	function spriteForType(type: string): SpriteDto | undefined {
		const id = selections[type];
		return catalog.find((s) => s.id === id);
	}
</script>

<div style="display: grid; grid-template-columns: 300px 1fr; gap: 2rem; align-items: start;">
	<div>
		<div style="position: relative; width: 256px; height: 256px; border: 1px solid #aaa; background: #fafafa;">
			{#each selectedInLayerOrder() as sprite (sprite.id)}
				<img src={sprite.assetPath} alt={sprite.description} style="position: absolute; top: 0; left: 0; width: 100%; height: 100%;" />
			{/each}
		</div>
		<button type="button" onclick={randomize} style="margin-top: 1rem; width: 256px;">Randomize</button>
	</div>

	<div>
		{#each TYPE_DISPLAY_ORDER as type (type)}
			{@const sprite = spriteForType(type)}
			{#if sprite}
				<div style="display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0; border-bottom: 1px solid #777;">
					<span style="width: 110px; font-weight: 600;">{type}</span>
					<button type="button" onclick={() => cycle(type, -1)} aria-label="Previous {type}">Left</button>
					<span style="flex: 1; text-align: center; font-family: monospace;">{sprite.name}</span>
					<button type="button" onclick={() => cycle(type, 1)} aria-label="Next {type}">Right</button>
				</div>
			{/if}
		{/each}
	</div>
</div>