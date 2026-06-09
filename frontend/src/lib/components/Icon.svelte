<script lang="ts">
	let { name, size = 24 }: { name: string; size?: number } = $props();
</script>

<svg
	class="icon"
	viewBox="0 0 24 24"
	width={size}
	height={size}
	fill="none"
	stroke="currentColor"
	stroke-width="2"
	stroke-linecap="round"
	stroke-linejoin="round"
	aria-hidden="true"
>
	{#if name === 'arrow-left'}
		<path d="m15 18-6-6 6-6" />
	{:else if name === 'arrow-right'}
		<path d="m9 18 6-6-6-6" />
	{:else if name === 'dice'}
		<rect x="3" y="3" width="18" height="18" rx="3" />
		<circle cx="8" cy="8" r="1.3" fill="currentColor" stroke="none" />
		<circle cx="12" cy="12" r="1.3" fill="currentColor" stroke="none" />
		<circle cx="16" cy="16" r="1.3" fill="currentColor" stroke="none" />
	{:else if name === 'trophy'}
		<path d="M6 9H4.5a2.5 2.5 0 0 1 0-5H6" />
		<path d="M18 9h1.5a2.5 2.5 0 0 0 0-5H18" />
		<path d="M4 22h16" />
		<path d="M10 14.66V17c0 .55-.47.98-.97 1.21C7.85 18.75 7 20.24 7 22" />
		<path d="M14 14.66V17c0 .55.47.98.97 1.21C16.15 18.75 17 20.24 17 22" />
		<path d="M18 2H6v7a6 6 0 0 0 12 0V2Z" />
	{:else if name === 'hero'}
		<path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2" />
		<circle cx="12" cy="7" r="4" />
	{:else if name === 'heroes'}
		<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
		<circle cx="9" cy="7" r="4" />
		<path d="M22 21v-2a4 4 0 0 0-3-3.87" />
		<path d="M16 3.13a4 4 0 0 1 0 7.75" />
	{:else if name === 'create'}
		<path d="M20.24 12.24a6 6 0 0 0-8.49-8.49L5 10.5V19h8.5z" />
		<path d="M16 8 2 22" />
		<path d="M17.5 15H9" />
	{:else if name === 'play'}
		<polygon points="6 3 20 12 6 21" fill="currentColor" stroke="none" />
	{:else if name === 'library'}
		<path d="M12 7v14" />
		<path
			d="M3 18a1 1 0 0 1-1-1V4a1 1 0 0 1 1-1h5a4 4 0 0 1 4 4 4 4 0 0 1 4-4h5a1 1 0 0 1 1 1v13a1 1 0 0 1-1 1h-6a3 3 0 0 0-3 3 3 3 0 0 0-3-3z"
		/>
	{:else if name === 'save'}
		<path d="M15.2 3a2 2 0 0 1 1.4.6l3.8 3.8a2 2 0 0 1 .6 1.4V19a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2z" />
		<path d="M17 21v-7a1 1 0 0 0-1-1H8a1 1 0 0 0-1 1v7" />
		<path d="M7 3v4a1 1 0 0 0 1 1h7" />
	{:else if name === 'close'}
		<path d="M18 6 6 18" />
		<path d="m6 6 12 12" />
	{/if}
</svg>

<style>
    .icon {
        display: block;
        flex: none;
    }
</style>
