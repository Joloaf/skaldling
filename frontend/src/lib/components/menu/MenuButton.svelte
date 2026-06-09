<script lang="ts">
	import Icon from '$lib/components/Icon.svelte';

	let {
		title,
		subtitle = '',
		href = '',
		icon = '',
		disabled = false
	}: {
		title: string;
		subtitle?: string;
		href?: string;
		icon?: string;
		disabled?: boolean;
	} = $props();
</script>

{#if disabled}
	<div class="menu-button" aria-disabled="true">
		{#if icon}<span class="icon" aria-hidden="true"><Icon name={icon} size={22} /></span>{/if}
		<span class="text">
			<span class="title">{title}</span>
			{#if subtitle}<span class="subtitle">{subtitle}</span>{/if}
		</span>
	</div>
{:else}
	<a class="menu-button" {href}>
		{#if icon}<span class="icon" aria-hidden="true"><Icon name={icon} size={22} /></span>{/if}
		<span class="text">
			<span class="title">{title}</span>
			{#if subtitle}<span class="subtitle">{subtitle}</span>{/if}
		</span>
	</a>
{/if}

<style>
    .menu-button {
        display: flex;
        align-items: center;
        gap: 1rem;
        width: 100%;
        padding: 1rem 1.25rem;
        border: 1px solid var(--border, #e6dcc4);
        border-radius: var(--radius, 12px);
        background: var(--surface, #fbf5e9);
        color: var(--text, #2a2018);
        text-decoration: none;
        text-align: left;
        box-sizing: border-box;
        transition: box-shadow 0.2s ease, border-color 0.2s ease;
    }
    a.menu-button:hover,
    a.menu-button:focus-visible {
        border-color: var(--accent, #46aab0);
        box-shadow:
                0 0 0 1px var(--accent, #46aab0),
                0 6px 28px color-mix(in srgb, var(--accent, #46aab0) 40%, transparent);
    }
    a.menu-button:focus-visible {
        outline: 2px solid var(--accent, #46aab0);
        outline-offset: 2px;
    }
    .menu-button[aria-disabled='true'] {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .icon {
        flex: none;
        display: grid;
        place-items: center;
        width: 44px;
        height: 44px;
        border-radius: 10px;
        background: var(--accent-tint, rgba(91, 122, 58, 0.14));
        color: var(--accent, #46aab0);
    }

    .text { display: flex; flex-direction: column; }
    .title { font-weight: 700; font-size: 1.1rem; }
    .subtitle { font-size: 0.85rem; color: var(--text-muted, #6b5d4a); margin-top: 0.1rem; }
</style>