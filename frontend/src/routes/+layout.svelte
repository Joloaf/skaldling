<script lang="ts">
	import favicon from '$lib/assets/favicon.svg';

	let { children } = $props();
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<nav style="padding: 1rem; border-bottom: 1px solid #ddd; display: flex; gap: 1rem;">
	<a href="/">Create hero</a>
	<a href="/sprites">Sprites catalog</a>
</nav>

<main style="padding: 1rem;">
	{@render children()}
</main>
